using CommuteMate.DTO;
using CommuteMate.Interfaces;
using NetTopologySuite.Geometries;
using System.Net.Http.Json;
using System.Text;
using CommuteMate.Utilities;

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
                            string distance;
                            if (walk.distance < 1)
                                distance = (walk.distance * 100).ToString() + " Meter/s";
                            else
                                distance = walk.ToString() + " KiloMeter/s";
                            string duration;
                            if (walk.duration < 1)
                                duration = (walk.duration * 60).ToString() + " Minutes/s";
                            else
                                duration = walk.ToString() + " Hour/s";
                            string action = walk.instruction + distance;
                            string instruction = "From " + walk.from + " to " + walk.to + " for " + duration;


                            //var walkGeom = _mapServices.ConvertToNtsGeometry(walk.geometry);
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
                                    rideDistance = (ride.distance * 100).ToString() + " Meter/s";
                                else
                                    rideDistance = ride.ToString() + " KiloMeter/s";
                                string rideDuration;
                                if (ride.duration < 1)
                                    rideDuration = (ride.duration * 60).ToString() + " Minutes/s";
                                else
                                    rideDuration = ride.ToString() + " Hour/s";
                                string rideAction = ride.instruction + distance;
                                string rideInstruction = "From " + ride.from + " to " + ride.to + " for " + duration;
                                //var rideGeom = _mapServices.ConvertToNtsGeometry(ride.geometry);
                                steps.Add(new RouteStep
                                {
                                    Action = rideAction,
                                    Instruction = rideInstruction,
                                    StepGeometry = ride.geometry
                                });
                                //geometries.Enqueue(rideGeom);
                            }
                        }

                        //create summary
                        string totalDistance;
                        if (properties.summary.distance < 1)
                            totalDistance = (properties.summary.distance * 100).ToString() + "Meter/s";
                        else
                            totalDistance = properties.summary.distance.ToString() + "KiloMeter/s";
                        string totalDuration;
                        if (properties.summary.duration < 1)
                            totalDuration = (properties.summary.duration * 60).ToString() + "Minutes/s";
                        else
                            totalDuration = properties.summary.duration.ToString() + "Hour/s";

                        var totalFare = "P" + properties.summary.fare.ToString();
                        var summary = new PathSummary
                        {
                            TotalDistance = totalDistance,
                            TotalDuration = totalDuration,
                            TotalFare = totalFare,
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
                return null;
            }
        }
    }
}
