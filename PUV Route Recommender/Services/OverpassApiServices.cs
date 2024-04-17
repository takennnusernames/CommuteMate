using NetTopologySuite.Geometries;
using CommuteMate.DTO;
using CommuteMate.Interfaces;
using System.Net.Http.Json;
using Linestring = NetTopologySuite.Geometries.LineString;


namespace CommuteMate.Services
{
    public class OverpassApiServices : IOverpassApiServices
    {
        HttpClient _httpClient;
        readonly IStreetService _streetService;
        readonly IRouteService _routeService;
        public OverpassApiServices(
            IStreetService streetService,
            IRouteService routeService)
        {
            _httpClient = new HttpClient();
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
                        foreach (var member in element.members)
                        {
                            var points = new List<Coordinate>();
                            foreach (var coordinate in member.geometry)
                            {
                                points.Add(new Coordinate(coordinate.lon, coordinate.lat));
                            }
                            string linestringWKT = _streetService.StreetListToWkt(points);
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
                        await _routeService.UpdateRouteAsync(route);
                    }
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
                            string linestringWKT = _streetService.StreetListToWkt(points);
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
                    string linestringWKT = _streetService.StreetListToWkt(points);
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
    }
}
