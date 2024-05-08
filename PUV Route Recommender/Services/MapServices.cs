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
using NetTopologySuite.LinearReferencing;
using Svg;
using NetTopologySuite.Operation.Distance;
using Point = NetTopologySuite.Geometries.Point;

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

        public async Task<ORSDirectionsDTO> GetDirectionsAsync(Coordinate origin, Coordinate destination)
        {
            Console.WriteLine("Getting Directions");
            try
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();

                //OpenRouteService Directions 
                var orsRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/v2/directions/driving-car/geojson");
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
                Console.WriteLine("Error in getting Direction:", ex.Message);
                throw;
            }
        }
        public async Task<List<PathData>> GetOptions(Feature feature)
        {
            Console.WriteLine("Getting Path");
            //List<RouteAction> paths = [];
            var streetList = await _overpassApiServices.GeometryToStreetListAsync(feature.geometry);

            //remove redunduncy of streets
            var streets = streetList.Select(tuple => tuple.Item1).GroupBy(s => s.Name).Select(g => g.First()).ToList();

            var routesQueues = await GetRoutesQueue(streets);
            var pathOptions = await CreatePath(streets, routesQueues);
            var options = new List<PathData>();
            foreach (var option in pathOptions)
            {
                options.Add(await CreatePathData(option.Item1,option.Item2,option.Item3));
            }
            return options;
        }
        public async Task<List<(List<Street>, Queue<Route>, List<IEnumerable<Edge<Coordinate>>>)>> CreatePath(List<Street> streets, Queue<List<RouteQueue>> routesQueues)
        {
            List< (List<Street>, Queue<Route>, List<IEnumerable<Edge<Coordinate>>>)> pathOptions = [];
            if (routesQueues.Count == 0)
            {
                pathOptions.Add((streets, new Queue<Route>(), new List<IEnumerable<Edge<Coordinate>>>()));
                return pathOptions;
            }

            var routesQueue = routesQueues.Dequeue();
            var bestRoutes = await GetBestRoute(routesQueue);

            foreach (var route in bestRoutes)
            {
                var puvRoute = route.Item2;
                var puvPath = route.Item1;

                var streetStart = streets.FindIndex(s => s.Osm_Id.Equals(puvRoute.startStreet.Osm_Id));
                if (streetStart == -1)
                    continue;

                var streetEnd = streets.FindIndex(s => s.Osm_Id.Equals(puvRoute.endStreet.Osm_Id));
                if (streetEnd == -1)
                    continue;

                var newStreets = new List<Street>(streets);
                var newRoutesQueues = new Queue<List<RouteQueue>>(routesQueues);
                //remove streets to be changed by puv route
                newStreets.RemoveRange(streetStart, streetEnd - streetStart);

                var createdPaths = await CreatePath(newStreets, newRoutesQueues);

                // Append the current PUV route to each created path
                if(createdPaths.Count > 0)
                    foreach (var createdPath in createdPaths)
                    {
                        createdPath.Item2.Enqueue(puvRoute.route);
                        createdPath.Item3.Add(puvPath);
                        pathOptions.Add(createdPath);
                    }
                else
                {
                    Queue<Route> routeList = new();
                    routeList.Enqueue(puvRoute.route);
                    List < IEnumerable < Edge < Coordinate >>> paths = [puvPath];
                    pathOptions.Add((newStreets, routeList, paths));
                }
            }
            return pathOptions;
        }
        public Task<PathData> CreatePathData(List<Street> walking, Queue<Route> puvs, List<IEnumerable<Edge<Coordinate>>> shortestPaths)
        {
            double totalWalkingDistance = 0;

            foreach (var street in walking)
            {
                List<Coordinate> coordinates = street.Coordinates;
                double totalLength = CalculateTotalLength(coordinates);

                totalWalkingDistance += totalLength;
            }

            double totalRidingDistance = 0;
            foreach(var path in shortestPaths)
            {
                double pathDistance = 0;
                foreach(var edge in path)
                {
                    List<Coordinate> coordinates = [];
                    coordinates.Add(edge.Source);
                    coordinates.Add(edge.Target);

                    double totalLength = CalculateTotalLength(coordinates);

                    pathDistance += totalLength;
                }
                totalRidingDistance += pathDistance;
            }

            double totalPathDistance = totalWalkingDistance + totalRidingDistance;
            double totalFare = 12.00;
            if(totalRidingDistance > 5)
            {
                var succeedingDistance = (int)totalRidingDistance% 5;
                var succeedingFare = succeedingDistance * 1.8;
                totalFare += succeedingFare;
            }

            PathData pathData = new PathData {
                streets = walking,
                puvs = puvs,
                totalFare = totalFare,
                totalDistance = totalPathDistance
            };
            return Task.FromResult(pathData);

        }
        public async Task<Queue<List<RouteQueue>>> GetRoutesQueue(List<Street> streets)
        {
            Console.WriteLine("Getting RoutesQueue");
            //queue for puv route and number of streets that it intersects
            Queue<Street> walkQueue = new();
            Queue<List<RouteQueue>> routesQueue = new();

            foreach (var street in streets)
            {
                //get puv routes that pass through the street
                var relatedRoutes = await _overpassApiServices.RetrieveRelatedRoutes(street.Osm_Id);
                if (relatedRoutes.Count > 0)
                {
                    List<RouteQueue> routes = new();
                    foreach (var route in relatedRoutes)
                    {
                        //check if route is already in the queue
                        if (routesQueue.Any(q => q.Any(r => r.route.Osm_Id == route.Osm_Id)))
                        {
                            var routeList = routesQueue.FirstOrDefault(q => q.Any(l => l.route.Equals(route)));
                            //increase count of route to determine best route
                            var tuple = routeList.FirstOrDefault(item => item.route.Equals(route));
                            if (tuple != default)
                            {
                                var index = routeList.IndexOf(tuple);
                                var updatedTuple = new RouteQueue
                                {
                                    route = tuple.route,
                                    intersectCount = tuple.intersectCount + 1,
                                    startStreet = tuple.startStreet,
                                    endStreet = street
                                };
                                routeList[index] = updatedTuple;
                            }
                        }
                        else
                            routes.Add(new RouteQueue
                            {
                                route = route,
                                intersectCount = 0,
                                startStreet = street,
                                endStreet = street
                            });
                    }
                    //create the queue for the routes
                    if (routes.Count > 0)
                        routesQueue.Enqueue(routes);
                }
                else
                {
                    walkQueue.Enqueue(street);
                }
            }
            return routesQueue;
        }

        public async Task<List<(IEnumerable<Edge<Coordinate>>, RouteQueue)>> GetBestRoute(List<RouteQueue> routes)
        {
            Console.WriteLine("Getting Best Route");
            List<(IEnumerable<Edge<Coordinate>>, RouteQueue) > bestRoute = [];
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

                var shortestPath = _routeService.GetShortetstPath(graph, routeStart.Coordinates.First(), routeEnd.Coordinates.Last());

                bestRoute.Add(new(shortestPath, route));

            }

            bestRoute = bestRoute
                .GroupBy(r => r.Item1.Count())
                .OrderByDescending(group => group.Key)
                .FirstOrDefault().ToList();

            return bestRoute;

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
