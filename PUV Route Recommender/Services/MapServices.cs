using CommuteMate.Interfaces;
using Mapsui;
using Mapsui.Projections;
using Tiling = Mapsui.Tiling;
using Map = Mapsui.Map;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Limiting;
using Mapsui.Nts;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using System.Net.Http.Json;
using CommuteMate.DTO;
using Color = Mapsui.Styles.Color;
using Mapsui.Nts.Extensions;
using System.Text;
using Microsoft.Maui.Controls;
using System.Linq;
using NominatimAPI;
using System.Collections.Generic;
using NetTopologySuite.IO.Converters;
using System;

namespace CommuteMate.Services
{
    public class MapServices : IMapServices
    {
        HttpClient _httpClient;
        CancellationTokenSource _cancellationTokenSource;
        IRouteService _routeService;
        IStreetService _streetService;
        IOverpassApiServices _overpassApiServices;
        public MapServices(IRouteService routeService, IStreetService streetService, IOverpassApiServices overpassApiServices) 
        {
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
            _routeService = routeService;
            _streetService = streetService;
            _overpassApiServices = overpassApiServices;
        }

        public async Task<Map> CreateMapAsync()
        {
            var map = new Map();
            map.Layers.Add(Tiling.OpenStreetMap.CreateTileLayer());

            var panBounds = GetLimitsOfCebuCity();
            map.Layers.Add(CreatePanBoundsLayer(panBounds));

            map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
            map.Navigator.RotationLock = true;
            map.Navigator.OverridePanBounds = panBounds;
            map.Home = n => n.ZoomToBox(panBounds);
            return await Task.FromResult(map);
        }
        public async Task<Location> GetLocationAsync(string addresss)
        {
                var geoLocation = await Geocoding.Default.GetLocationsAsync(addresss);
                Location location = geoLocation?.FirstOrDefault();
                return location;
        }
        public async Task<List<string>> SearchLocationAsync(string input)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource = new CancellationTokenSource();
            List<string> locationNames = [];
            string url = "https://nominatim.openstreetmap.org/search";
            var minX = 123.77;
            var minY = 10.25;
            var maxX = 123.9309;
            var maxY = 10.4953;
            var response = await _httpClient.GetAsync($"{url}?q={input}&bounded=1&format=jsonv2&viewbox={minX},{minY},{maxX},{maxY}", _cancellationTokenSource.Token);

            if (response.IsSuccessStatusCode)
            {
                var locations = await response.Content.ReadFromJsonAsync<List<LocationDTO>>();
                foreach ( var location in locations )
                {
                    locationNames.Add(location.display_name);
                }
            }
            return locationNames;

        }

        public async Task GetDirectionsAsync(Coordinate origin, Coordinate destination, Map map)
        {
            try
            {
                await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource = new CancellationTokenSource();

                //OpenRouteService Directions 
                var orsRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/v2/directions/driving-car/geojson");
                orsRequest.Headers.Add("Authorization", "Bearer 5b3ce3597851110001cf6248d82ffd7c0abb468cbe0cd0bf61653d82");
                var coordinates = new double[][] {
            [origin.X, origin.Y],
            [destination.X, destination.Y]
            };

                var alternativeRoutes = new
                {
                    target_count = 3,
                    weight_factor = 1.4,
                    share_factor = 0.6
                };

                var requestData = new
                {
                    coordinates,
                    alternative_routes = alternativeRoutes
                };

                var jsonString = System.Text.Json.JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
                orsRequest.Content = content;

                //Get route from origin to destination
                var orsResponse = await _httpClient.SendAsync(orsRequest, _cancellationTokenSource.Token);
                if (orsResponse.IsSuccessStatusCode)
                {
                    ORSDirectionsDTO orsResponseData = await orsResponse.Content.ReadFromJsonAsync<ORSDirectionsDTO>();

                    foreach (var feature in orsResponseData.features)
                    {
                        List<(Coordinate, OSMDataDTO)> coordinateRelatedRoutesList = new List<(Coordinate, OSMDataDTO)>();
                        //queue for puv route and number of streets that it intersects
                        Queue<List<(Route, int, Street, Street)>> routesQueue = new();

                        //get street information for the geometry
                        var streetList = await _overpassApiServices.GeometryToStreetListAsync(feature.geometry);

                        //remove redunduncy of streets
                        var streets = streetList.Select(tuple => tuple.Item1).GroupBy(s => s.Name).Select(g => g.First()).ToList();
                        foreach (var street in streets)
                        {
                            //get puv routes that pass through the street
                            var relatedRoutes = await _overpassApiServices.RetrieveRelatedRoutes(street.Osm_Id);
                            if (relatedRoutes.Count > 0)
                            {
                                List<(Route, int, Street, Street)> routes = new();
                                foreach (var route in relatedRoutes)
                                {
                                    //check if route is already in the queue
                                    if (routesQueue.Any(q => q.Any(r => r.Item1.Osm_Id == route.Osm_Id)))
                                    {
                                        var routeList = routesQueue.FirstOrDefault(q => q.Any(l => l.Item1.Equals(route)));
                                        //increase count of route to determine best route
                                        var tuple = routeList.FirstOrDefault(item => item.Item1.Equals(route));
                                        if (tuple != default)
                                        {
                                            var index = routeList.IndexOf(tuple);
                                            var updatedTuple = (tuple.Item1, tuple.Item2 + 1, tuple.Item3, street);
                                            routeList[index] = updatedTuple;
                                        }
                                    }
                                    else
                                        routes.Add(new(route, 0, street, street));
                                }
                                //create the queue for the routes
                                if (routes.Count > 0)
                                    routesQueue.Enqueue(routes);
                            }
                        }
                        List<string> lineStrings = [];
                        while (routesQueue.Count > 0)
                        {
                            var routes = routesQueue.Dequeue();
                            var highestIntersectRoute = routes
                                                    .GroupBy(tuple => tuple.Item2) // Group tuples by their integer value
                                                    .OrderByDescending(group => group.Key) // Order groups by descending integer value
                                                    .FirstOrDefault() // Select the first (i.e., highest) group
                                                    .ToList();
                            //iterate each route
                            foreach (var route in highestIntersectRoute)
                            {
                                //get all streets for the route
                                var relatedStreets = await _overpassApiServices.RetrieveStreetWithNodesAsync(route.Item1.Osm_Id);
                                //get streets of main route
                                var newPath = streetList.Select(l => l.Item1).ToList();
                                var streetStart = newPath.FindIndex(s => s.Osm_Id == route.Item3.Osm_Id);
                                var streetEnd = newPath.FindIndex(s => s.Osm_Id == route.Item4.Osm_Id);
                                //remove streets to be changed by puv route
                                newPath.RemoveRange((streetStart + 1), ((streetEnd - streetStart) - 1));

                                //sequence streets of route
                                await _streetService.StreetSequenceOrder(relatedStreets);
                                var routeStart = route.Item3;
                                var routeEnd = route.Item4;
                                var routeStartIndex = relatedStreets.FindIndex(tuple => tuple.Osm_Id == routeStart.Osm_Id);
                                var routeEndIndex = relatedStreets.FindIndex(tuple => tuple.Osm_Id == routeEnd.Osm_Id);
                                //select streets from the start up to the end of intersection
                                var streetsInRange = relatedStreets
                                        .Skip(routeStartIndex)
                                        .Take(routeEndIndex - routeStartIndex + 1)
                                        .ToList();

                                //insert new path
                                //newPath.InsertRange((streetStart + 1), streetsInRange);
                                var wkts = newPath.Select(s => s.GeometryWKT).ToList();
                                lineStrings.AddRange(wkts);
                                break;
                            }
                            break;

                        }
                        //var lineString = _streetService.StreetListToWkt(streetList.Select(tuple => tuple.Item2).ToList());
                        await addLineString(map, lineStrings);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in getting Direction:", ex.Message);
                throw;
            }
        }

        public string LineStringToWKT(LineString lineString)
        {
            StringBuilder wktBuilder = new StringBuilder();
            wktBuilder.Append("LINESTRING (");

            for (int i = 0; i < lineString.NumPoints; i++)
            {
                Coordinate coord = lineString.GetCoordinateN(i);
                wktBuilder.Append(coord.X).Append(" ").Append(coord.Y);
                if (i < lineString.NumPoints - 1)
                    wktBuilder.Append(", ");
            }

            wktBuilder.Append(")");
            return wktBuilder.ToString();
        }




        public async Task<Map> addLineString(Map map, string WKTString)
        {
            var lineStringLayer = CreateLineStringLayer(WKTString, CreateLineStringStyle());
            map.Layers.Add(lineStringLayer);
            map.Home = n => n.CenterOnAndZoomTo(lineStringLayer.Extent!.Centroid, 200);
            return await Task.FromResult(map);
        }

        public async Task<Map> addLineString(Map map, List<string> WKTStrings)
        {
            try
            {
                foreach (string wktString in WKTStrings)
                {
                    var lineStringLayer = CreateLineStringLayer(wktString, CreateLineStringStyle());
                    map.Layers.Add(lineStringLayer);
                    map.Home = n => n.CenterOnAndZoomTo(lineStringLayer.Extent!.Centroid, 200);
                }

                return await Task.FromResult(map);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error inc addLineString: ", ex.Message);
                throw;
            }
        }
        public static ILayer CreateLineStringLayer(string WKTString, IStyle style = null)
        {
            try
            {
                var lineString = (LineString)new WKTReader().Read(WKTString);
                //lineString = new LineString(lineString.Coordinates.Select(v => new Coordinate(v.Y, v.X)).ToArray());
                lineString = new LineString(lineString.Coordinates.Select(v => SphericalMercator.FromLonLat(v.Y, v.X).ToCoordinate()).ToArray());

                return new MemoryLayer
                {
                    Features = new[] { new GeometryFeature { Geometry = lineString } },
                    Name = "LineStringLayer",
                    Style = style

                };
            }
            
            catch(Exception ex)
            {
                Console.WriteLine("Error inc CreateLineStringLayer: ", ex.Message);
                throw;
            }
        }
        public static IStyle CreateLineStringStyle()
        {
            try
            {
                return new VectorStyle
                {
                    Fill = null,
                    Outline = null,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
                    Line = { Color = Color.FromString("YellowGreen"), Width = 4 }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inc CreateLineStringStyle: ", ex.Message);
                throw;
            }
        }



        //latitude longitude
        private const string WKTGr5 = "LINESTRING(10.298626 123.888948, 10.299175 123.888527, 10.299228 123.888606, 10.299363 123.888812, 10.299481 123.888997, 10.299566 123.889169, 10.299652 123.88939, 10.29978 123.889713, 10.299892 123.889991, 10.3001 123.890582, 10.300122 123.890699, 10.300125 123.890854, 10.300013 123.891243, 10.299956 123.891421, 10.299903 123.891586, 10.29974 123.89208, 10.299532 123.8928, 10.299294 123.893575, 10.299264 123.893706, 10.299217 123.893896, 10.299167 123.894086, 10.299013 123.894831, 10.298985 123.894993, 10.298907 123.895391, 10.298846 123.895508, 10.298793 123.895573, 10.298531 123.895652, 10.297982 123.89581, 10.297314 123.896007, 10.295648 123.896499, 10.295462 123.896519, 10.295573 123.896739, 10.295967 123.897667, 10.296103 123.897945, 10.296251 123.898191, 10.296303 123.898276, 10.296247 123.898403, 10.29619 123.898546, 10.296068 123.898856, 10.29579 123.899527, 10.295436 123.900331, 10.295261 123.900734, 10.295153 123.900961, 10.294884 123.901571, 10.294837 123.901667, 10.294562 123.902307, 10.294236 123.903039, 10.293909 123.903761, 10.293824 123.903952, 10.293584 123.904554, 10.293553 123.90462, 10.294184 123.904919, 10.294392 123.905018, 10.293906 123.905886, 10.293821 123.906038, 10.293779 123.906116, 10.293743 123.906183)";
        private static MRect GetLimitsOfCebuCity()
        {
            // Longitude and latitude of the southwest limit Cebu City
            var (minX, minY) = SphericalMercator.FromLonLat(123.77, 10.25);
            // Longitude and latitude of the northeast limit Cebu City
            var (maxX, maxY) = SphericalMercator.FromLonLat(123.95, 10.50); 
            return new MRect(minX, minY, maxX, maxY);
        }
        private static MemoryLayer CreatePanBoundsLayer(MRect panBounds)
        {
            // This layer is only for visualizing the pan bounds. It is not needed for the limiter.
            return new MemoryLayer("PanBounds")
            {
                Features = new[] { new RectFeature(panBounds) },
                Style = new VectorStyle() { Fill = null, Outline = new Pen(Mapsui.Styles.Color.Red, 3) { PenStyle = PenStyle.Dot } }
            };
        }
    }
}
