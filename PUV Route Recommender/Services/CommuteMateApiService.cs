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
            //_client.BaseAddress = new Uri("http://10.0.2.2:5005");
            _client.BaseAddress = new Uri("https://commutemateapi.azurewebsites.net/");
        }
        public async Task<List<Route>> GetRoutes()
        {
            try
            {
                var responseData = await _client.GetFromJsonAsync<List<RouteDTO>>("/api/Route");
                List<Route> routes = [];
                foreach (var data in responseData)
                {
                    var route = new Route
                    {
                        RouteId = data.Id,
                        OsmId = data.OsmId,
                        Code = data.RouteCode,
                        Name = data.RouteName
                    };
                    route = await _routeService.InsertRouteAsync(route);
                    routes.Add(route);
                }
                return routes;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return [];
            }
        }
        public async Task<List<Street>> GetRouteStreets(long osmId)
        {
            var responseData = await _client.GetFromJsonAsync<List<StreetDTO>>($"/api/RouteStreet/route/{osmId}");
            List<Street> streets = [];
            try
            {
                foreach (var data in responseData)
                {
                    streets.Add(new Street
                    {
                        StreetId = data.Id,
                        OsmId = data.OsmId,
                        Name = data.StreetName,
                        GeometryWKT = data.GeometryWKT
                    });
                }
                return streets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in retrieving streets: {ex.Message}");
                throw;
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
                        var stops = properties.segments[1];
                        var riding = properties.segments[2];
                        var walkingQueue = new Queue<Step>(walking.steps);
                        var stopQueue = new Queue<Step>(stops.steps);
                        var ridingQueue = new Queue<Step>(riding.steps);
                        var steps = new List<RouteStep>();

                        var pickup = new RouteStep();
                        var dropOff = new RouteStep();

                        Queue<NetTopologySuite.Geometries.Geometry> geometries = [];
                        //create steps
                        while (walkingQueue.Count > 0)
                        {
                            var walk = walkingQueue.Dequeue();
                            string distanceString = "";
                            double distance = 0;
                            if (walk.distance < 1)
                            {
                                distance = Math.Round(walk.distance * 1000,2);
                                distanceString = distance.ToString() + " Meter/s";
                            }
                            else
                            {
                                distance = walk.distance;
                                distanceString = distance.ToString() + " KiloMeter/s";
                            }
                            string duration;
                            if (walk.duration < 1.0)
                                duration = Math.Ceiling(walk.duration * 60).ToString() + " Minutes/s";
                            else
                                duration = walk.ToString() + " Hour/s";
                            string action = walk.instruction + " (" + distanceString + ")";
                            string instruction = "From " + walk.from + " to " + walk.to + " for " + duration;

                            if (distance >= 1)
                            {

                                steps.Add(new RouteStep
                                {
                                    Action = action,
                                    Instruction = instruction,
                                    StepGeometry = walk.geometry
                                });
                            }

                            //geometries.Enqueue(walkGeom);
                            if (ridingQueue.Count > 0)
                            {
                                var ride = ridingQueue.Dequeue();
                                if (stopQueue.Count > 0)
                                {
                                    var stop = stopQueue.Peek();
                                    if (stop.instruction.Contains("Transfer"))
                                    {
                                        stop = stopQueue.Dequeue();
                                        steps.Add(new RouteStep
                                        {
                                            Action = stop.instruction,
                                            Instruction = "Pay Fare of P" + ride.fare + " and Wait for next PUV at " + ride.from,
                                            StepGeometry = stop.geometry
                                        });
                                    }
                                    else if(stop.instruction.Contains("Pick"))
                                    {
                                        stop = stopQueue.Dequeue();
                                        steps.Add(new RouteStep
                                        {
                                            Action = stop.instruction,
                                            Instruction = "Wait for PUV at " + ride.from,
                                            StepGeometry = stop.geometry
                                        });
                                    }
                                }

                                string rideDistance;
                                if (ride.distance < 1)
                                    rideDistance = Math.Round(ride.distance * 1000,2).ToString() + " Meter/s";
                                else
                                    rideDistance = ride.distance.ToString() + " KiloMeter/s";
                                string rideDuration;
                                if (ride.duration < 1.0)
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


                                if (stopQueue.Count > 0)
                                {
                                    var stop = stopQueue.Peek();
                                    if (stop.instruction.Contains("Transfer"))
                                    {
                                        stop = stopQueue.Dequeue();
                                        steps.Add(new RouteStep
                                        {
                                            Action = stop.instruction,
                                            Instruction = "Pay Fare of P" + ride.fare + " and Wait for next PUV at " + ride.to,
                                            StepGeometry = stop.geometry
                                        });
                                    }
                                    else if (stop.instruction.Contains("Drop"))
                                    {
                                        stop = stopQueue.Dequeue();
                                        steps.Add(new RouteStep
                                        {
                                            Action = stop.instruction,
                                            Instruction = "Pay Fare of P" + ride.fare + " at " + ride.to,
                                            StepGeometry = stop.geometry
                                        });
                                    }
                                }
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
                return null;
            }
        }
        public async Task<List<string>> SearchRoute(string text)
        {
            var responseData = await _client.GetFromJsonAsync<List<string>>($"/api/route/search?input={text}");
            return responseData;
        }

        public async Task<List<Vehicle>> GetVehicles()
        {
            try
            {
                var responseData = await _client.GetFromJsonAsync<List<PuvDTO>>("/api/Puv");
                List<Vehicle> vehicles = [];
                foreach (var data in responseData)
                {
                    //var driveUrl = data.SampleImage;
                    //var regex = new System.Text.RegularExpressions.Regex(@"(?:drive\.google\.com\/file\/d\/|d\/|id=)([^\/?]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    //string imageLink;
                    //var match = regex.Match(driveUrl);
                    //if (match.Success)
                    //{
                    //    var fileId = match.Groups[1].Value;
                    //    imageLink = $"https://drive.google.com/uc?export=view&id={fileId}";
                    //}
                    //else
                    //{
                    //    throw new ArgumentException("Invalid Google Drive URL format.", nameof(driveUrl));
                    //}
                    var vehicle = new Vehicle
                    {
                        VehicleID = data.Id,
                        Type = data.Type,
                        Info = new VehicleInfo
                        {
                            MinimumFare = data.MinimumFare,
                            MinimumKM = data.MinimumKm,
                            Comfortability = data.Comfortability,
                            FareRate = data.FareIncrease
                        },
                        ImageFileName = data.SampleImage
                    };
                    vehicles.Add(vehicle);
                }
                return vehicles;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
