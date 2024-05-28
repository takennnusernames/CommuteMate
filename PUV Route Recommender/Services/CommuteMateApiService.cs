using CommuteMate.DTO;
using CommuteMate.Interfaces;
using NetTopologySuite.Geometries;
using System.Net.Http.Json;
using System.Text;
using CommuteMate.Utilities;
using Point = NetTopologySuite.Geometries.Point;

namespace CommuteMate.Services
{
    public class CommuteMateApiService : ICommuteMateApiService
    {
        HttpClient _client;
        IMapServices _mapServices;
        IStreetService _streetService;
        IRouteService _routeService;
        public CommuteMateApiService(IMapServices mapServices, IStreetService streetService, IRouteService routeService)
        {
            _routeService = routeService;
            _streetService = streetService;
            _mapServices = mapServices;
            _client = new HttpClient();
            _client.BaseAddress = new Uri("http://10.0.2.2:5005");
            //_client.BaseAddress = new Uri("https://commutemateapi.azurewebsites.net/");
        }
        public async Task<List<Route>> GetRoutes()
        {
            var responseData = await _client.GetFromJsonAsync<List<RouteDTO>>("/api/Route");
            List<Route> routes = [];
            foreach(var data in responseData)
            {
                var route = new Route
                {
                    RouteId = data.Id,
                    Osm_Id = data.OsmId,
                    Code = data.RouteCode,
                    Name = data.RouteName
                };
                route = await _routeService.InsertRouteAsync(route);
                routes.Add(route);
            }
            return routes;
        }
        public async Task<List<Street>> GetRouteStreets(long osmId)
        {
            var responseData = await _client.GetFromJsonAsync<List<StreetDTO>>($"/api/RouteStreet/route/{osmId}");
            List<Street> streets = [];
            try
            {
                foreach (var data in responseData)
                {
                    var points = new List<Coordinate>();
                    foreach (var coordinate in data.Geometry.Coordinates) //geometry
                    {
                        points.Add(new Coordinate(coordinate[0], coordinate[1]));
                    }
                    LineString lineString = new LineString(points.ToArray());
                    streets.Add(new Street
                    {
                        StreetId = data.Id,
                        Osm_Id = data.OsmId,
                        Name = data.StreetName
                    });
                }
                return streets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in retrieving streets: {ex.Message}");
                return null;
            }
        }

        public async Task<List<RoutePath>> GetPath(Coordinate origin, Coordinate destination)
        {
            var requestBody = new PathRequest{
                Origin = new PathCoordinate(origin.X,origin.Y),
                Destination = new PathCoordinate(destination.X,destination.Y)
            };

            var jsonString = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            try
            {
                List<RoutePath> routePaths = [];
                var response = await _client.PostAsync("/pathRequest/", content);
                response.EnsureSuccessStatusCode();

                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        Converters = { new GeometryConverter() }
                    };

                    var responseData = await response.Content.ReadFromJsonAsyncWithCustomConverter<PathDataDTO>(options);

                    //var responseData = JsonSerializer.Deserialize<List<PathDataDTO>>(responseContent);
                    foreach (var path in responseData.features)
                    {
                        var properties = path.properties;
                        var walking = properties.segments[0];
                        var riding = properties.segments[1];
                        var walkingQueue = new Queue<Step>(walking.steps);
                        var ridingQueue = new Queue<Step>(riding.steps);
                        var steps = new List<RouteStep>();

                        Queue<NetTopologySuite.Geometries.Geometry> geometries = [];
                        //create steps
                        while (walkingQueue.Count > 0)
                        {
                            var walk = walkingQueue.Dequeue();
                            string distanceString = "";
                            double distance = 0;
                            if (walk.distance < 1)
                            {
                                distance = Math.Round(walk.distance * 100,2);
                                distanceString = distance.ToString() + " Meter/s";
                            }
                            else
                            {
                                distance = walk.distance;
                                distanceString = distance.ToString() + " KiloMeter/s";
                            }
                            string duration;
                            if (walk.duration < 1)
                                duration = Math.Ceiling(walk.duration * 60).ToString() + " Minutes/s";
                            else
                                duration = walk.ToString() + " Hour/s";
                            string action = walk.instruction + " (" + distanceString + ")";
                            string instruction = "From " + walk.from + " to " + walk.to + " for " + duration;

                            if(distance >= 1)
                                steps.Add(new RouteStep
                                {
                                    Action = action,
                                    Instruction = instruction,
                                    StepGeometry = walk.geometry
                                });

                            else if(ridingQueue.Count>0)
                                steps.Add(new RouteStep
                                {
                                    Action = "Pickup",
                                    Instruction = "Wait for PUV at " + walk.to,
                                    StepGeometry = new Point(walk.geometry.Coordinates.Last())
                                });
                            else
                                steps.Add(new RouteStep
                                {
                                    Action = action,
                                    Instruction = instruction,
                                    StepGeometry = walk.geometry
                                });

                            //geometries.Enqueue(walkGeom);
                            if (ridingQueue.Count > 0)
                            {

                                var ride = ridingQueue.Dequeue();

                                string rideDistance;
                                if (ride.distance < 1)
                                    rideDistance = Math.Round(ride.distance * 100,2).ToString() + " Meter/s";
                                else
                                    rideDistance = ride.distance.ToString() + " KiloMeter/s";
                                string rideDuration;
                                if (ride.duration < 1)
                                    rideDuration = Math.Ceiling(ride.duration * 60).ToString() + " Minutes/s";
                                else
                                    rideDuration = ride.ToString() + " Hour/s";
                                string rideAction = ride.instruction + " PUV " + ride.code + " (" + rideDistance + ")";
                                string rideInstruction = "From " + ride.from + " to " + ride.to + " for " + rideDuration;
                                //var rideGeom = _mapServices.ConvertToNtsGeometry(ride.geometry);
                                steps.Add(new RouteStep
                                {
                                    Action = rideAction,
                                    Instruction = rideInstruction,
                                    StepGeometry = ride.geometry
                                });

                                steps.Add(new RouteStep
                                {
                                    Action = "Drop off",
                                    Instruction = "Pay the fare of P" + ride.fare,
                                    StepGeometry = new Point(walk.geometry.Coordinates.Last())
                                });
                                //geometries.Enqueue(rideGeom);
                            }
                        }

                        var summary = new PathSummary
                        {
                            TotalDistance = properties.summary.distance,
                            TotalDuration = properties.summary.duration,
                            TotalFare = properties.summary.fare,
                            PUVCodes = riding.steps.Select(r => r.code).ToList()
                        };

                        routePaths.Add(new RoutePath
                        {
                            Steps = steps,
                            Summary = summary,
                            RouteGeometry = path.geometry
                        });
                    }
                    return routePaths;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return null;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                throw new Exception($"Error in Getting Available Routes:", ex);
            }
        }
    }
}
