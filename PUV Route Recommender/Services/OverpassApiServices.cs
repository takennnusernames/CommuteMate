using NetTopologySuite.Geometries;
using CommuteMate.DTO;
using CommuteMate.Interfaces;
using System.Net.Http.Json;
using Linestring = NetTopologySuite.Geometries.LineString;
using OsmSharp;
using Newtonsoft.Json.Linq;
using NominatimAPI;
using Point = NetTopologySuite.Geometries.Point;
using System.Linq;
using Microsoft.Maui.Controls;


namespace CommuteMate.Services
{
    public class OverpassApiServices : IOverpassApiServices
    {
        HttpClient _httpClient;
        CancellationTokenSource _cancellationTokenSource;
        readonly IStreetService _streetService;
        readonly IRouteService _routeService;
        public OverpassApiServices(
            IStreetService streetService,
            IRouteService routeService)
        {
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
            _streetService = streetService;
            _routeService = routeService;
        }

        public async Task RetrieveOverpassRoutesAsync()
        {
            string url = "https://overpass-api.de/api/interpreter";
            string query = @"
                [out:json];
                area(3612455830)->.searchArea; /*cebu city area*/
                relation[""route""=""bus""](area.searchArea);
                out geom;";
            var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}");

            if (response.IsSuccessStatusCode)
            {
                var osmData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                try
                {
                    foreach (var element in osmData.elements)
                    {
                        if (element.type == "relation")
                        {
                            var code = element.tags.TryGetValue("ref", out string routeCode) ? routeCode : "No Code";
                            if (code.Contains("Ceres"))
                                continue;
                            Route route = new Route
                            {
                                Osm_Id = element.id,
                                Code = code,
                                Name = element.tags.TryGetValue("name", out string name) ? name : "No Name",
                                StreetNameSaved = false,
                                Streets = []
                            };
                            route = await _routeService.InsertRouteAsync(route);
                            try
                            {
                                foreach (var member in element.members)
                                {
                                    if(member.type == "way")
                                    {

                                        try
                                        {
                                            var points = new List<Coordinate>();
                                            try
                                            {
                                                foreach (var coordinate in member.geometry)
                                                {
                                                    points.Add(new Coordinate(coordinate.lon, coordinate.lat));
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine($"Error in saving streets. Error: {ex.Message}");
                                            }
                                            string linestringWKT = await _streetService.StreetListToWkt(points);
                                            Street street = new Street
                                            {
                                                Osm_Id = member.@ref,
                                                Name = "No Tags",
                                                GeometryWKT = linestringWKT,
                                                Routes = []
                                            };
                                            street.Routes.Add(route);
                                            await _streetService.InsertStreetAsync(street);

                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine($"Error in member {member.ToString}: {ex.Message}");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error in saving streets. Error: {ex.Message}");
                            }
                            await _routeService.UpdateRouteAsync(route);
                        }
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"error in saving route: {ex.Message}");
                }
            }
        }
        public async Task RetrieveOverpassRouteStreetNamesAsync(long OsmId, int routeId)
        {
            var route = await _routeService.GetRouteByIdAsync(routeId);
            if (!route.StreetNameSaved)
            {
                string url = "https://overpass-api.de/api/interpreter";
                string query = $@"
                [out:json];
                relation({OsmId});
                way(r);
                out tags;";
                var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}");
                if (response.IsSuccessStatusCode)
                {
                    var osmData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                    foreach (var element in osmData.elements)
                    {
                        var street = await _streetService.GetStreetByWayIdAsync(element.id);
                        if (street.Name == "No Tags")
                        {
                            street.Name = element.tags.TryGetValue("name", out string name) ? name : "No Name";
                            await _streetService.UpdateStreetAsync(street);
                        }
                    }
                    route.StreetNameSaved = true;
                    await _routeService.UpdateRouteAsync(route);
                }
            }
        }
        public async Task RetrieveOverpassRouteStreetsAsync()
        {
            var routes = await _routeService.GetAllRoutesAsync();
            foreach(Route route in routes)
            {
                string url = "https://overpass-api.de/api/interpreter";
                string query = $@"
                [out:json];
                relation({route.Osm_Id});
                way(r);
                out geom;";
                var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}");
                if (response.IsSuccessStatusCode)
                {
                    var osmData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                    foreach (var element in osmData.elements)
                    {
                        var street = await _streetService.GetStreetByWayIdAsync(element.id);
                        if (street != null)
                        {
                            if (route.Streets.Any(s => s.Osm_Id == element.id))
                                continue;
                            route.Streets.Add(street);
                        }
                        else
                        {
                            var points = new List<Coordinate>();
                            foreach (var coordinate in element.geometry)
                            {
                                points.Add(new Coordinate(coordinate.lon, coordinate.lat));
                            }
                            string linestringWKT = await _streetService.StreetListToWkt(points);
                            Street newStreet = new()
                            {
                                Osm_Id = element.id,
                                Name = element.tags.TryGetValue("name", out string name) ? name : "No Name",
                                GeometryWKT = linestringWKT,
                                Routes = []
                            };
                            newStreet.Routes.Add(route);
                            street = await _streetService.InsertStreetAsync(newStreet);
                            route.Streets.Add(street);
                        }
                    }
                    await _routeService.UpdateRouteAsync(route);
                }
            }
        }
        public async Task<Street> RetrieveOverpassStreetAsync(long OsmId)
        {
            Street street = new();
            string overpassUrl = "https://overpass-api.de/api/interpreter";
            string query = $@"
                                [out:json];
                                way({OsmId});
                                out geom;";

            var overpassResponse = await _httpClient.GetAsync($"{overpassUrl}?data={Uri.EscapeDataString(query)}");
            if (overpassResponse.IsSuccessStatusCode)
            {
                OSMDataDTO overpassResponseData = await overpassResponse.Content.ReadFromJsonAsync<OSMDataDTO>();
                foreach (var element in overpassResponseData.elements)
                {
                    var points = new List<Coordinate>();
                    foreach (var coordinate in element.geometry)
                    {
                        points.Add(new Coordinate(coordinate.lon, coordinate.lat));
                    }
                    string linestringWKT = await _streetService.StreetListToWkt(points);
                    street = await _streetService.InsertStreetAsync(new Street
                    {
                        Osm_Id = element.id,
                        Name = element.tags.TryGetValue("name", out string name) ? name : "No Name",
                        GeometryWKT = linestringWKT
                    });
                }
                return street;
            }
            throw new Exception("Error! HttpRequest Unsuccessful");
        }

        public async Task<List<Route>> RetrieveRelatedRoutes(long OsmId)
        {
            try
            {
                await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource = new CancellationTokenSource();
                string url = "https://overpass-api.de/api/interpreter";
                string query = $@"
                [out:json];
                way({OsmId});
                rel(bw)[""route""=""bus""];
                out geom;";
                var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}", _cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                List<Route> relatedRoutes = [];
                foreach (var data in responseData.elements)
                {
                    Route route = new Route
                    {
                        Osm_Id = data.id,
                        Code = data.tags.TryGetValue("ref", out string code) ? code : "No Code",
                        Name = data.tags.TryGetValue("name", out string name) ? name : "No Name",
                        StreetNameSaved = false,
                        Streets = []
                    };
                    relatedRoutes.Add(route);
                }

                return relatedRoutes;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RetrievRelatedRoutes: ", ex.Message);
                throw;
            }

        }

        public async Task<List<Street>> RetrieveRelatedStreetsAsync(long OsmId)
        {
            try
            {
                await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource = new CancellationTokenSource();
                string url = "https://overpass-api.de/api/interpreter";
                string query = $@"
                [out:json];
                relation({OsmId});
                way(r);
                out geom skel;";
                var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}", _cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                List<Street> relatedStreets = [];
                foreach (var element in responseData.elements)
                {
                    var points = new List<Coordinate>();
                    foreach (var coordinate in element.geometry)
                    {
                        points.Add(new Coordinate(coordinate.lon, coordinate.lat));
                    }
                    string linestringWKT = await _streetService.StreetListToWkt(points);
                    Street street = new Street
                    {
                        Osm_Id = element.id,
                        GeometryWKT = linestringWKT
                    };
                    relatedStreets.Add(street);
                }
                return relatedStreets;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in RetrieveRelatedStret",ex.Message);
                throw;
            }
        }
        public async Task<List<StreetWithNode>> RetrieveStreetWithNodesAsync(long OsmId)
        {
            try
            {
                await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource = new CancellationTokenSource();
                string url = "https://overpass-api.de/api/interpreter";
                string query = $@"
                [out:json];
                relation({OsmId});
                way(r);
                out geom skel;";
                var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}", _cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                List<StreetWithNode> relatedStreets = [];
                foreach (var element in responseData.elements)
                {
                    StreetWithNode street = new StreetWithNode
                    {
                        Osm_Id = element.id,
                        Nodes = []
                    };
                    street.Nodes.AddRange(element.nodes);
                    relatedStreets.Add(street);
                }
                return relatedStreets;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RetrieveRelatedStret", ex.Message);
                throw;
            }
        }

        public async Task<List<(Street, Coordinate, int)>> GeometryToStreetListAsync(DTO.Geometry geometry)
        {
            try
            {
                List<(Street street, Coordinate, int rank)> streetList = new();
                int iteration = 0;
                foreach (var coordinate in geometry.coordinates)
                {
                    var data = await ReverseGeocodeSearch(coordinate[1], coordinate[0]);

                    if (data.category == "highway")
                    {
                        var street = await RetrieveOverpassStreetAsync(data.osm_id);
                        streetList.Add(new(street, new Coordinate(coordinate[0], coordinate[1]), iteration));
                        iteration++;
                    }
                    else
                    {
                        Console.WriteLine($"{data.category}, {geometry.coordinates}");
                    }
                }
                return streetList;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GeometryToStreet:", ex.Message);
                throw;
            }
        }

        public async Task<LocationDTO> ReverseGeocodeSearch(double latitude, double longitude)
        {
            try
            {
                await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource = new CancellationTokenSource();
                string NominatimBaseUrl = "https://nominatim.openstreetmap.org/reverse";

                string url = $"{NominatimBaseUrl}?lat={latitude}&lon={longitude}&format=jsonv2";

                HttpResponseMessage response = await _httpClient.GetAsync(url, _cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<LocationDTO>();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Eror in ReverseGeocod: ", ex.Message);
                throw;
            }
        }
    }
}
