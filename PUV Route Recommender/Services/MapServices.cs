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
using QuickGraph;
using Coordinate = NetTopologySuite.Geometries.Coordinate;
using ExCSS;
using Mapsui.Providers.Wms;
using System.Collections.Generic;
using Point = NetTopologySuite.Geometries.Point;
using BruTile.Wms;
using Exception = System.Exception;
using System;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Maui.Maps;
using GoogleMap = Microsoft.Maui.Controls.Maps.Map;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.ApplicationModel;
using NetTopologySuite.GeometriesGraph;
using Colors = Microsoft.Maui.Graphics.Colors;

namespace CommuteMate.Services
{
    public class MapServices : IMapServices
    {
        readonly HttpClient _httpClient;
        CancellationTokenSource _cancellationTokenSource;
        IRouteService _routeService;
        IStreetService _streetService;
        IOverpassApiServices _overpassApiServices;
        IGeolocation _geolocation;
        public MapServices(IRouteService routeService, IStreetService streetService, IOverpassApiServices overpassApiServices, IGeolocation geolocation) 
        {
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
            _routeService = routeService;
            _streetService = streetService;
            _overpassApiServices = overpassApiServices;
            _geolocation = geolocation;
        }

        public Task<Map> CreateMapAsync()
        {
            try
            {
                var map = new Map();

                map.Layers.Add(Tiling.OpenStreetMap.CreateTileLayer());

                var panBounds = GetLimitsOfCebuCity();
                map.Layers.Add(CreatePanBoundsLayer(panBounds));

                map.Navigator.Limiter = new ViewportLimiterKeepWithinExtent();
                map.Navigator.RotationLock = true;
                map.Navigator.OverridePanBounds = panBounds;
                map.Home = n => n.ZoomToBox(panBounds);

                return Task.FromResult(map);
            }
            catch
            {
                throw new System.Exception("Failed to Create Map");
            }
        }

        public Task CreateGoogleMapAsync(GoogleMap map)
        {
            Location location = new Location(10.3157, 123.8854);
            MapSpan mapSpan = new MapSpan(location, 0.05, 0.05);
            map.MoveToRegion(mapSpan);

            return Task.FromResult(map);
        }

        public async Task<Pin> AddGooglePin(Location location, GoogleMap map, string label)
        {
            var pin = new Pin
            {
                Type = PinType.Place,
                Location = location,
                Label = label,
                Address = location.Longitude.ToString() + ", " + location.Latitude.ToString()
            };
            map.Pins.Add(pin);
            return await Task.FromResult(pin);

        }
        public Task<Pin> AddGooglePin(LocationDetails location, GoogleMap map)
        {
            var pin = new Pin
            {
                Type = PinType.Place,
                Location = new Location
                {
                    Latitude = location.Coordinate.Y,
                    Longitude = location.Coordinate.X
                },
                Label = location.Name,
                Address = location.Coordinate.X.ToString() + ", " + location.Coordinate.Y.ToString()
            };

            MapSpan mapSpan = new MapSpan(pin.Location, 0.01, 0.01);
            map.Pins.Add(pin);
            map.MoveToRegion(mapSpan);
            return Task.FromResult(pin);
        }
        public Task RemoveGooglePin(Pin pin, GoogleMap map)
        {
            map.Pins.Remove(pin);
            return Task.FromResult(map);
        }
        public async Task<Polyline> AddGooglePolyline(Geometry geometry, GoogleMap map, string action)
        {
            List<Location> locations = [];
            if (geometry.GeometryType.Equals("MultiLineString"))
            {
                var multiLineString = (MultiLineString)geometry;

                foreach (var lineString in multiLineString.Geometries)
                {
                    if(lineString.GeometryType == "Point")
                    {
                        var point = (Point)geometry;
                        locations.Add(new Location(point.X, point.Y));
                    }
                    else
                    {
                        locations.AddRange(ConvertLineStringToLocations((LineString)lineString));
                    }
                }
            }
            else if (geometry.GeometryType.Equals("LineString"))
            {
                locations.AddRange(ConvertLineStringToLocations((LineString)geometry));
            }
            else
            {
                var point = (Point)geometry;
                locations.Add(new Location(point.X, point.Y));
            }
            var polyline = new Polyline();
            if (action.Contains("Walk"))
            {
                polyline.StrokeColor = Colors.Black;
                polyline.StrokeWidth = 3;
            }
            else if (action.Contains("Ride"))
            {
                polyline.StrokeColor = Colors.Orange;
                polyline.StrokeWidth = 6;
            }
            
            foreach (var position in locations)
            {
                polyline.Geopath.Add(position);
            }
            map.MapElements.Add(polyline);

            return polyline;

        }

        IEnumerable<Location> ConvertLineStringToLocations(LineString lineString)
        {
            List<Location> locations = [];
            foreach (var coordinate in lineString.Coordinates)
            {
                locations.Add(new Location(coordinate.X, coordinate.Y));
            }
            return locations;
        }

        public async Task<Map> AddPin(Map map, LocationDetails location)
        {

            var point = new Point(SphericalMercator.FromLonLat(location.Coordinate.X,location.Coordinate.Y).ToCoordinate());
            var testPoint = new Point(location.Coordinate);
            var testPoint2 = new Point(location.Coordinate.Y,location.Coordinate.X);

            var layer = new MemoryLayer
            {
                Features = new[] { new GeometryFeature { Geometry = point } },
                Name = "PinLayer",
                Style = SymbolStyles.CreatePinStyle()
            };
            map.Layers.Add(layer);

            map.Home = n => n.CenterOnAndZoomTo(layer.Extent!.Centroid, 9);
            await Task.Delay(100);

            return map;

        }


        public async Task<LocationDetails> GetCurrentLocationAsync()
        {
            var location = await _geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(30)
            });

            if (location != null)
            {
                var latitude = location.Latitude;
                var longitude = location.Longitude;

                var minX = 123.77;
                var minY = 10.25;
                var maxX = 123.9309;
                var maxY = 10.4953;

                if (latitude >= minY && latitude <= maxY && longitude >= minX && longitude <= maxX)
                {
                    LocationDetails details = new LocationDetails
                    {
                        Coordinate = new Coordinate(latitude, longitude),
                        Name = new string(latitude.ToString() + "," + longitude.ToString())
                    };

                    return details;
                }
                else
                    return null;
            }
            else
            {
                throw new Exception("Unable to find Location");
            }
        }
        public async Task<List<LocationDetails>> SearchLocationAsync(string input)
        {
            await _cancellationTokenSource.CancelAsync();
            _cancellationTokenSource = new CancellationTokenSource();
            List<LocationDetails> locationList = [];
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
                    Coordinate coordinate = new Coordinate(double.Parse(location.lon), double.Parse(location.lat));
                    locationList.Add(new LocationDetails
                    {
                        Name = location.display_name,
                        Coordinate = coordinate
                    });
                }
            }
            return locationList;

        }

        public async Task<List<LocationDetails>> GoogleSearchLocationAsync(string input)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://places.googleapis.com/v1/places:searchText");
            request.Headers.Add("X-Goog-Api-Key", "AIzaSyC_zmye1jCAnMGsWfevUPmN8UzlRz6mu_g");
            request.Headers.Add("X-Goog-FieldMask", "places.displayName,places.location");
            double latitude = 10.25;
            double longitude = 123.77;
            double highLatitude = 10.4953;
            double highLongitude = 123.9309;

            var json = $@"{{
                  ""textQuery"" : ""{input}"",
                  ""locationBias"": {{
                        ""rectangle"": {{
                            ""low"": {{
                                ""latitude"": {latitude},
                                ""longitude"": {longitude}
                                }},
                            ""high"": {{
                                ""latitude"": {highLatitude},
                                ""longitude"": {highLongitude}
                                }}
                            }}
                    }}
                }}";

            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            request.Content = content;
            try
            {
                var response = await _httpClient.SendAsync(request, _cancellationTokenSource.Token);
                response.EnsureSuccessStatusCode();

                List<LocationDetails> locationList = [];

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<GoogleLocationDTO>();
                    foreach (var place in responseData.places)
                    {
                        Coordinate coordinate = new Coordinate(place.location.longitude, place.location.latitude);
                        locationList.Add(new LocationDetails
                        {
                            Name = place.displayName.text,
                            Coordinate = coordinate
                        });
                    }
                }
                return locationList;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in Searching Location", ex);
            }

        }

        //remove
        public async Task<ORSDirectionsDTO> GetDirectionsAsync(Coordinate origin, Coordinate destination)
        {
            Console.WriteLine("Getting Directions");
            try
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                //OpenRouteService Directions 
                var orsRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/v2/directions/foot-walking/geojson");
                orsRequest.Headers.Add("Authorization", "Bearer 5b3ce3597851110001cf6248d82ffd7c0abb468cbe0cd0bf61653d82");
                var coordinates = new double[][] {
                [origin.X, origin.Y],
                [destination.X, destination.Y]
                };

                var requestData = new
                {
                    coordinates
                };

                var jsonString = System.Text.Json.JsonSerializer.Serialize(requestData);
                var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");
                orsRequest.Content = content;

                //Get route from origin to destination
                var orsResponse = await _httpClient.SendAsync(orsRequest, _cancellationTokenSource.Token);
                if (orsResponse.IsSuccessStatusCode)
                {
                    return await orsResponse.Content.ReadFromJsonAsync<ORSDirectionsDTO>();
                }
                throw new Exception("HttpRequest Failed");

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        //remove
        public async Task<List<PathData>> GetOptions(Feature feature)
        {
            Console.WriteLine("Getting Path");
            try
            {
                var streetList = await _overpassApiServices.GeometryToStreetListAsync(feature.geometry);

                //remove redunduncy of streets
                var streets = streetList.Select(tuple => tuple.Item1).GroupBy(s => s.Osm_Id).Select(g => g.First()).ToList();

                var routesQueues = await GetRoutesQueue(streetList.Select(s => s.Item1).ToList());

                var pathOptions = await CreatePath(streetList.Select(s => s.Item1).ToList(), routesQueues);

                var minStreetCount = pathOptions.Min(o => o.Item1.Count);
                var bestWalkOptions = pathOptions.Where(o => o.Item1.Count == minStreetCount).ToList();

                var minPuvCount = pathOptions.Min(o => o.Item2.Count);
                var bestPuvOptions = pathOptions.Where(o => o.Item2.Count == minPuvCount).ToList();

                List<PathData> options = [];

                foreach (var option in bestPuvOptions)
                {
                    Queue<Route> routes = new();
                    Queue<List<Coordinate>> coordinateQueue = new();
                    List<Coordinate> coordinates = [];

                    var route = option.Item2.Dequeue();
                    var lastCoordinate = streetList.FirstOrDefault(s => s.Item1 == route.startStreet).Item2;
                    foreach (var street in option.Item1)
                    {
                        var index = streetList.FindIndex(s => s.Item1 == street);
                        var coordinate = streetList.FirstOrDefault(s => s.Item1 == street).Item2;
                        if(coordinate!=default)
                            if(!coordinates.Contains(coordinate))
                                coordinates.Add(coordinate);

                        var nextCoordinate = streetList.ElementAtOrDefault(index+1).Item2;
                        if (nextCoordinate != default)
                            if (!coordinates.Contains(nextCoordinate)) 
                                coordinates.Add(nextCoordinate);

                        if (coordinate == lastCoordinate)
                        {
                            coordinateQueue.Enqueue(coordinates);
                            routes.Enqueue(route.route);
                            if(option.Item2.Count > 0)
                            {
                                route = option.Item2.Dequeue();
                                lastCoordinate = streetList.FirstOrDefault(s => s.Item1 == route.startStreet).Item2;
                            }
                            coordinates = [];
                        }
                    }
                    coordinateQueue.Enqueue(coordinates);
                    options.Add(await CreatePathData(streets, coordinateQueue, routes, option.Item3));
                }

                foreach (var option in bestWalkOptions)
                {
                    Queue<Route> routes = new();
                    Queue<List<Coordinate>> coordinateQueue = new();
                    List<Coordinate> coordinates = [];
                    if (option.Item2.Count == 0)
                        continue;

                    var route = option.Item2.Dequeue();

                    var lastCoordinate = streetList.FirstOrDefault(s => s.Item1 == route.startStreet).Item2;
                    foreach (var street in option.Item1)
                    {
                        var index = streetList.FindIndex(s => s.Item1 == street);
                        var coordinate = streetList.FirstOrDefault(s => s.Item1 == street).Item2;
                        if (coordinate != default)
                            if (!coordinates.Contains(coordinate))
                                coordinates.Add(coordinate);

                        var nextCoordinate = streetList.ElementAtOrDefault(index + 1).Item2;
                        if (nextCoordinate != default)
                            if (!coordinates.Contains(nextCoordinate))
                                coordinates.Add(nextCoordinate);

                        if (coordinate == lastCoordinate)
                        {
                            coordinateQueue.Enqueue(coordinates);
                            routes.Enqueue(route.route);
                            if (option.Item2.Count > 0)
                            {
                                route = option.Item2.Dequeue();
                                lastCoordinate = streetList.FirstOrDefault(s => s.Item1 == route.startStreet).Item2;
                            }
                            coordinates = [];
                        }
                    }
                    coordinateQueue.Enqueue(coordinates);
                    options.Add(await CreatePathData(streets, coordinateQueue, routes, option.Item3));
                }
                    return options;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error in GetOptions Method: {ex.Message}");
                return null;
            }
        }
        //remove
        public async Task<List<(List<Street>, Queue<RouteQueue>, List<IEnumerable<Edge<Coordinate>>>)>> CreatePath(List<Street> streets, Queue<List<(IEnumerable<Edge<Coordinate>>, RouteQueue)>> routesQueues)
        {
            List< (List<Street>, Queue<RouteQueue>, List<IEnumerable<Edge<Coordinate>>>)> pathOptions = [];
            

            while(routesQueues.Count > 0)
            {
                var routesQueue = routesQueues.Dequeue();
                foreach (var route in routesQueue)
                {
                    var puvRoute = route.Item2;
                    if (route.Item1 == null)
                        continue;
                    var puvPath = route.Item1;

                    var streetStart = streets.FindIndex(s => s.Osm_Id.Equals(puvRoute.startStreet.Osm_Id));
                    if (streetStart == -1)
                        continue;

                    var streetEnd = streets.FindIndex(s => s.Osm_Id.Equals(puvRoute.endStreet.Osm_Id));
                    if (streetEnd == -1)
                        continue;

                    //remove streets to be changed by puv route
                    //var newStreets = streets.Take(streetStart + 1).ToList();
                    //newStreets.AddRange(streets.Skip(streetEnd + 1));

                    var newStreets = new List<Street>(streets);
                    newStreets.RemoveRange(streetStart + 1, (streetEnd - streetStart));

                    List<(List<Street>, Queue<RouteQueue>, List<IEnumerable<Edge<Coordinate>>>)> createdPaths = [];
                    if (routesQueues.Count == 0)
                    {
                        Queue<RouteQueue> routeList = new();
                        routeList.Enqueue(puvRoute);
                        List<IEnumerable<Edge<Coordinate>>> paths = [puvPath];
                        pathOptions.Add((newStreets, routeList, paths));
                        return pathOptions;
                    }
                    else
                        createdPaths = await CreatePath(newStreets, new Queue<List<(IEnumerable<Edge<Coordinate>>, RouteQueue)>>(routesQueues));

                    // Append the current PUV route to each created path
                    if (createdPaths.Count > 0)
                        foreach (var createdPath in createdPaths)
                        {
                            createdPath.Item2.Enqueue(puvRoute);
                            createdPath.Item3.Add(puvPath);
                            pathOptions.Add(createdPath);
                        }
                    else
                    {
                        Queue<RouteQueue> routeList = new();
                        routeList.Enqueue(puvRoute);
                        List<IEnumerable<Edge<Coordinate>>> paths = [puvPath];
                        pathOptions.Add((newStreets, routeList, paths));
                    }
                }
            }
            return pathOptions;
        }
        //remove
        public Task<PathData> CreatePathData(List<Street> streets, Queue<List<Coordinate>> walking, Queue<Route> puvs, List<IEnumerable<Edge<Coordinate>>> shortestPaths)
        {
            double totalWalkingDistance = 0;
            List<Edge<Coordinate>> completeWalkingPath = new();
            foreach(var walk in walking)
            {
                var graph = _routeService.CoordinatesToGraph(walk);
                double pathDistance = 0;
                foreach (var edge in graph.Edges)
                {
                    List<Coordinate> coordinates = [];
                    coordinates.Add(edge.Source);
                    coordinates.Add(edge.Target);

                    double totalLength = CalculateTotalLength(coordinates);

                    pathDistance += totalLength;

                    completeWalkingPath.Add(edge);
                }
                totalWalkingDistance += pathDistance;
            }
            double totalRidingDistance = 0;
            List< Edge < Coordinate >> completePuvPath = new();
            foreach (var path in shortestPaths)
            {
                double pathDistance = 0;
                foreach(var edge in path)
                {
                    List<Coordinate> coordinates = [];
                    coordinates.Add(edge.Source);
                    coordinates.Add(edge.Target);

                    double totalLength = CalculateTotalLength(coordinates);

                    pathDistance += totalLength;

                    completePuvPath.Add(edge);
                }
                totalRidingDistance += pathDistance;
            }

            double totalFare = Constants.traditionalPuvMinimumFare;
            //change per PUV
            if(totalRidingDistance > 5)
            {
                var succeedingDistance = (int)totalRidingDistance% 5;
                var succeedingFare = succeedingDistance * 1.8;
                totalFare += succeedingFare;
            }

            PathData pathData = new PathData {
                streets = streets,
                puvs = puvs,
                walkingPath = completeWalkingPath,
                puvShortestPaths = completePuvPath,
                totalFare = totalFare,
                totalWalkingDistance = totalWalkingDistance,
                totalPuvRideDistance = totalRidingDistance
            };
            return Task.FromResult(pathData);

        }
        public async Task<Queue<List<(IEnumerable<Edge<Coordinate>>, RouteQueue)>>> GetRoutesQueue(List<Street> streetList)
        {
            Console.WriteLine("Getting RoutesQueue");
            //queue for puv route and number of streets that it intersects
            Queue<List<RouteQueue>> routesQueue = new();

            var streets = streetList.GroupBy(s => s.Name).Select(g => g.First()).ToList();

            List<string> queries = [];
            foreach (var street in streetList)
            {
                queries.Add($@"way({street.Osm_Id});rel(bw)[""route""=""bus""];");
                //get puv routes that pass through the street
            }

            var relatedRoutes = await _overpassApiServices.RetrieveRelatedRoutes(queries);
            //if (relatedRoutes.Count > 0)
            //{
            //    List<RouteQueue> routes = new();
            //    foreach (var route in relatedRoutes)
            //    {
            //        //check if route is already in the queue
            //        if (routesQueue.Any(q => q.Any(r => r.route.Osm_Id == route.Osm_Id)))
            //        {
            //            var routeList = routesQueue.FirstOrDefault(q => q.Any(l => l.route.Equals(route)));
            //            //increase count of route to determine best route
            //            var tuple = routeList.FirstOrDefault(item => item.route.Equals(route));
            //            if (tuple != default)
            //            {
            //                var index = routeList.IndexOf(tuple);
            //                int intersect = 0;
            //                if (tuple.endStreet.Name != street.Name)
            //                    intersect = 1;
            //                var updatedTuple = new RouteQueue
            //                {
            //                    route = tuple.route,
            //                    intersectCount = tuple.intersectCount + intersect,
            //                    startStreet = tuple.startStreet,
            //                    endStreet = street
            //                };
            //                routeList[index] = updatedTuple;
            //            }
            //        }
            //        else
            //            routes.Add(new RouteQueue
            //            {
            //                route = route,
            //                intersectCount = 1,
            //                startStreet = street,
            //                endStreet = street
            //            });
            //    }
            //    //create the queue for the routes
            //    if (routes.Count > 0)
            //    {
            //        routesQueue.Enqueue(routes);
            //    }
            //}

            var newRoutesQueue = new Queue<List<RouteQueue>>();
            //remove all route with only one intersect
            while (routesQueue.Count > 0)
            {
                var q = routesQueue.Dequeue();
                var newQ = q.Where(r => r.intersectCount != 1).ToList();


                //foreach (var route in newQ)
                //{
                //    if (route.startStreet == route.endStreet)
                //        continue;
                //    var newEnd = lastStreets.Find(s => s.Name == route.endStreet.Name);
                //    route.endStreet = newEnd;
                //}

                if (newQ.Count > 0)
                    newRoutesQueue.Enqueue(newQ);
            }

            var bestRoutesQueue = new Queue<List<(IEnumerable<Edge<Coordinate>>, RouteQueue)>>();
            while (newRoutesQueue.Count > 0)
            {
                var q = newRoutesQueue.Dequeue();
                var bestRoutes = await GetBestRoute(q);
                if (bestRoutes.Count < 1)
                    continue;
                bestRoutesQueue.Enqueue(bestRoutes);
            }
            return bestRoutesQueue;
        }

        public async Task<List<(IEnumerable<Edge<Coordinate>>, RouteQueue)>> GetBestRoute(List<RouteQueue> routes)
        {
            Console.WriteLine("Getting Best Route");
            try
            {
                List<(IEnumerable<Edge<Coordinate>>, RouteQueue)> bestRoute = [];
                var highestIntersectRoute = routes
                                            .GroupBy(r => r.intersectCount) // Group tuples by their integer value
                                            .OrderByDescending(group => group.Key) // Order groups by descending integer value
                                            .FirstOrDefault() // Select the first (i.e., highest) group
                                            .ToList();
                //iterate each route

                foreach (var route in highestIntersectRoute)
                {
                    //get all streets for the route
                    var relatedStreets = await _overpassApiServices.RetrieveStreetWithCoordinatesAsync(route.route.Osm_Id);
                    var graph = await _routeService.StreetToGraph(relatedStreets, route.route.Osm_Id);

                    //start of puv
                    var startStreet = route.startStreet;
                    var routeStart = relatedStreets.Where(r => r.Osm_Id == startStreet.Osm_Id).FirstOrDefault();
                    //end of puv


                    var endStreet = route.endStreet;
                    var routeEnd = relatedStreets.Where(r => r.Osm_Id == endStreet.Osm_Id).FirstOrDefault();

                    if (startStreet.Osm_Id == endStreet.Osm_Id)
                        continue;
                    var shortestPath = _routeService.GetShortetstPath(graph, routeStart.Coordinates.First(), routeEnd.Coordinates.Last());
                    if (shortestPath != null)
                        bestRoute.Add(new(shortestPath, route));

                }

                if (bestRoute.Count > 1)
                {
                    bestRoute = [.. bestRoute
                .GroupBy(r => r.Item1.Count())
                .OrderByDescending(group => group.Key)
                .FirstOrDefault()];
                }

                return bestRoute;
            }
            catch(WebException ex)
            {
                throw new Exception($"Error in BestRoute: {ex.Message}");
            }

        }
        public string LineStringToWKT(LineString lineString)
        {
            Console.WriteLine("Converting To WKT");
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

       
        public async Task<Map> addLineString(Map map, string WKTString, string style)
        {
            ILayer lineStringLayer;
            if (style == "straight")
                lineStringLayer = CreateLineStringLayer(WKTString, CreateStraightLineStringStyle("orange"));
            else if(style == "dotted")
                lineStringLayer = CreateLineStringLayer(WKTString, CreateStraightLineStringStyle("black"));
            else
                lineStringLayer = CreateLineStringLayer(WKTString, CreateLineStringStyle());

            map.Layers.Add(lineStringLayer);
            map.Home = n => n.CenterOnAndZoomTo(lineStringLayer.Extent!.Centroid, 9);
            return await Task.FromResult(map);
        }

        public async Task<Map> addGeometry(Map map, Geometry geometry, string style)
        {
            ILayer lineStringLayer;

            if (geometry.GeometryType.Equals("MultiLineString"))
            {
                var multiLineString = (MultiLineString)geometry;
                var lineStrings = multiLineString.Geometries.Select(line =>
                {
                    var coordinates = ((LineString)line).Coordinates.Select(v => SphericalMercator.FromLonLat(v.Y, v.X).ToCoordinate()).ToArray();
                    return new LineString(coordinates);
                }).ToArray();
                foreach (var lineString in lineStrings)
                {
                    if (style == "straight")
                        lineStringLayer = CreateLineStringLayer(lineString, CreateStraightLineStringStyle("orange"));
                    else if (style == "dotted")
                        lineStringLayer = CreateLineStringLayer(lineString, CreateStraightLineStringStyle("black"));
                    else
                        lineStringLayer = CreateLineStringLayer(lineString, CreateLineStringStyle());

                    map.Layers.Add(lineStringLayer);
                }

                return await Task.FromResult(map);
            }
            else if (geometry.GeometryType.Equals("LineString"))
            {
                var lineString = new LineString(geometry.Coordinates.Select(v => SphericalMercator.FromLonLat(v.Y, v.X).ToCoordinate()).ToArray());

                if (style == "straight")
                    lineStringLayer = CreateLineStringLayer(lineString, CreateStraightLineStringStyle("orange"));
                else if (style == "dotted")
                    lineStringLayer = CreateLineStringLayer(lineString, CreateDottedLineStringStyle("black"));
                else
                    lineStringLayer = CreateLineStringLayer(lineString, CreateLineStringStyle());

                map.Layers.Add(lineStringLayer);

                return await Task.FromResult(map);
            }
            else
            {
                var point = new Point(geometry.Coordinates.Select(v => SphericalMercator.FromLonLat(v.Y, v.X).ToCoordinate()).FirstOrDefault());
                
                lineStringLayer = CreatePointLayer(point, CreatePointStyle("blue"));
                map.Layers.Add(lineStringLayer);

                return await Task.FromResult(map);
            }



        }

        public async Task<Map> addLineString(Map map, NetTopologySuite.Geometries.Geometry lineString, string style)
        {
            ILayer lineStringLayer;
            if (style == "straight")
                lineStringLayer = CreateLineStringLayer(lineString, CreateStraightLineStringStyle("orange"));
            else if (style == "dotted")
                lineStringLayer = CreateLineStringLayer(lineString, CreateStraightLineStringStyle("black"));
            else
                lineStringLayer = CreateLineStringLayer(lineString, CreateLineStringStyle());


            

            map.Layers.Add(lineStringLayer);
            map.Home = n => n.CenterOnAndZoomTo(lineStringLayer.Extent!.Centroid, 9);
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

        public async Task addPath(Map map, IEnumerable<Edge<Coordinate>> path, string style)
        {
            var components = new Dictionary<Coordinate, List<Edge<Coordinate>>>();
            foreach (var edge in path)
            {
                if (!components.TryGetValue(edge.Source, out var list))
                {
                    list = new List<Edge<Coordinate>>();
                    components.Add(edge.Source, list);
                }
                list.Add(edge);
            }

            // Draw each connected component separately
            foreach (var component in components.Values)
            {
                var coordinates = new List<Coordinate>();
                foreach (var edge in component)
                {
                    coordinates.Add(edge.Source);
                    coordinates.Add(edge.Target);
                }
                string lineString = await _streetService.StreetListToWkt(coordinates);
                await addLineString(map, lineString, style);
            }
        }
        public async Task addPath(Map map, List<NetTopologySuite.Geometries.Geometry> geometries, string style)
        {
            foreach(var geometry in geometries)
            {
                if(geometry.GeometryType == "LineString")
                {
                    await addLineString(map, geometry, style);
                }
            }             
        }
        public static ILayer CreateLineStringLayer(string WKTString, IStyle style = null)
        {
            try
            {
                var lineString = (LineString)new WKTReader().Read(WKTString);
                //geometry = new LineString(geometry.Coordinates.Select(v => new Coordinate(v.Y, v.X)).ToArray());
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

        public static ILayer CreateLineStringLayer(LineString lineString, IStyle style = null)
        {
            try
            {
                return new MemoryLayer
                {
                    Features = new[] { new GeometryFeature { Geometry = lineString } },
                    Name = "LineStringLayer",
                    Style = style
                };
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateLineStringLayer: ", ex.Message);
                throw;
            }
        }

        public static ILayer CreatePointLayer(Point point, IStyle style = null)
        {
            try
            {
                return new MemoryLayer
                {
                    Features = new[] { new GeometryFeature { Geometry = point } },
                    Name = "PointLayer",
                    Style = style
                };
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateLineStringLayer: ", ex.Message);
                throw;
            }
        }

        public static ILayer CreateLineStringLayer(Geometry lineString, IStyle style = null)
        {
            try
            {
                return new MemoryLayer
                {
                    Features = new[] { new GeometryFeature { Geometry = lineString } },
                    Name = "LineStringLayer",
                    Style = style
                };
            }

            catch (Exception ex)
            {
                Console.WriteLine("Error in CreateLineStringLayer: ", ex.Message);
                throw;
            }
        }

        public static ILayer CreatePinLayer(string WKTString, IStyle style = null)
        {
            try
            {
                var lineString = (LineString)new WKTReader().Read(WKTString);
                //geometry = new LineString(geometry.Coordinates.Select(v => new Coordinate(v.Y, v.X)).ToArray());
                lineString = new LineString(lineString.Coordinates.Select(v => SphericalMercator.FromLonLat(v.Y, v.X).ToCoordinate()).ToArray());

                return new MemoryLayer
                {
                    Features = new[] { new GeometryFeature { Geometry = lineString } },
                    Name = "LineStringLayer",
                    Style = style

                };
            }

            catch (Exception ex)
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
                    Line = { Color = Color.FromString("green"), Width = 4 }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inc CreateStraightLineStringStyle: ", ex.Message);
                throw;
            }
        }

       
        public static IStyle CreateStraightLineStringStyle(string color)
        {
            try
            {
                return new VectorStyle
                {
                    Fill = null,
                    Outline = null,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
                    Line = { Color = Color.FromString(color), Width = 4 }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inc CreateStraightLineStringStyle: ", ex.Message);
                throw;
            }
        }

        public static IStyle CreateDottedLineStringStyle(string color)
        {
            try
            {
                return new VectorStyle
                {
                    Fill = null,
                    Outline = null,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
                    Line = { Color = Color.FromString(color), Width = 4 }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inc CreateStraightLineStringStyle: ", ex.Message);
                throw;
            }
        }

        public static IStyle CreatePointStyle(string color)
        {
            try
            {
                return new VectorStyle
                {
                    Fill = null,
                    Outline = null,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
                    Line = { Color = Color.FromString(color), Width = 4 }
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error inc CreateStraightLineStringStyle: ", ex.Message);
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

        static double CalculateTotalLength(List<Coordinate> coordinates)
        {
            double totalDistance = 0;
            for (int i = 0; i < coordinates.Count - 1; i++)
            {
                double distance = CalculateHaversineDistance(coordinates[i], coordinates[i + 1]);
                totalDistance += distance;
            }
            return totalDistance;
        }

        // Function to calculate the distance between two coordinates using the Haversine formula
        static double CalculateHaversineDistance(Coordinate coord1, Coordinate coord2)
        {
            const double EarthRadiusKm = 6371; // Earth radius in kilometers

            double lat1Rad = DegreesToRadians(coord1.Y);
            double lon1Rad = DegreesToRadians(coord1.X);
            double lat2Rad = DegreesToRadians(coord2.Y);
            double lon2Rad = DegreesToRadians(coord2.X);

            double dLat = lat2Rad - lat1Rad;
            double dLon = lon2Rad - lon1Rad;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            double distance = EarthRadiusKm * c;

            return distance;
        }

        // Function to convert degrees to radians
        static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

    }
}
