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
            _cancellationTokenSource.Cancel();
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
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            if (_routeService.CountRoutesAsync().Result == 0)
                throw new Exception("Routes Table is empty");

            var orsRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/v2/directions/driving-car/geojson");

            //OpenRouteService Directions API Token
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
            var response = await _httpClient.SendAsync(orsRequest);
            response.EnsureSuccessStatusCode();

            ORSDirectionsDTO responseData = await response.Content.ReadFromJsonAsync<ORSDirectionsDTO>();
            List<string> lineStrings = [];
            List<List<PathAction>> possibleRoute = [];

            List<Route> possiblePuv = [];
            List<List<OSMCoordinate>> puvGeom = [];

            //loop each coordinate
            foreach (var feature in responseData.features)
            {
                List<Street> streetPath = [];
                List<Street> ride = [];
                Queue<List<Route>> puvRoutesQueue = [];
                List<Route> tempRoutes = [];

                List<Coordinate> points = [];
                //get each coordinate
                foreach (var coordinate in feature.geometry.coordinates)
                {
                    //search for osm object connected to the coordinate
                    string url = $"https://nominatim.openstreetmap.org/reverse?format=jsonv2&lat={Uri.EscapeDataString(coordinate[1].ToString())}&lon={Uri.EscapeDataString(coordinate[0].ToString())}";
                    var nominatimRequest = new HttpRequestMessage(HttpMethod.Get, url);
                    var nominatimResponse = await _httpClient.SendAsync(nominatimRequest);
                    nominatimResponse.EnsureSuccessStatusCode();
                    var nominatimResponseData = await nominatimResponse.Content.ReadFromJsonAsync<LocationDTO>();
                    if( nominatimResponseData != null)
                    {
                        //get street objects that is part of the path
                        if (nominatimResponseData.category == "highway" )
                        {
                            var street = await _streetService.GetStreetByWayIdAsync(nominatimResponseData.osm_id);

                            street ??= await _overpassApiServices.RetrieveOverpassStreetAsync(nominatimResponseData.osm_id);
                            
                            if (!streetPath.Contains(street))
                                streetPath.Add(street);
                            points.Add(new Coordinate(coordinate[1], coordinate[0]));

                            //create queue for available routes in the path
                            if(!puvRoutesQueue.Contains(street.Routes))
                                puvRoutesQueue.Enqueue([.. street.Routes]);
                        }
                    }
                }
                while(puvRoutesQueue.Count > 0)
                {
                    var routes = puvRoutesQueue.Dequeue();
                    foreach(var route in routes)
                    {
                        var intersect = route.Streets.Intersect(streetPath);
                    }
                }
                string linestringWKT = _streetService.StreetListToWkt(points);
                lineStrings.Add(linestringWKT);
            }
            foreach(var linestringWKT in lineStrings)
                await addLineString(map, linestringWKT);
            return;
        }
        public async Task<Map> addLineString(Map map, string WKTString)
        {
            var lineStringLayer = CreateLineStringLayer(WKTString, CreateLineStringStyle());
            map.Layers.Add(lineStringLayer);
            map.Home = n => n.CenterOnAndZoomTo(lineStringLayer.Extent!.Centroid, 200);
            return await Task.FromResult(map);
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
        


        public static ILayer CreateLineStringLayer(string WKTString, IStyle style = null)
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
        public static IStyle CreateLineStringStyle()
        {
            return new VectorStyle
            {
                Fill = null,
                Outline = null,
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
                Line = { Color = Color.FromString("YellowGreen"), Width = 4 }
            };
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
