using PUV_Route_Recommender.Interfaces;
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
using PUV_Route_Recommender.DTO;
using Color = Mapsui.Styles.Color;
using Mapsui.Nts.Extensions;
using Newtonsoft.Json;
using NetTopologySuite.Features;
using Mapsui.Providers.Wms;
using Point = NetTopologySuite.Geometries.Point;
using NetTopologySuite.Algorithm;
using Geom = NetTopologySuite.Geometries.Geometry;
using System.Text;

namespace PUV_Route_Recommender.Services
{
    public class MapServices : IMapServices
    {
        HttpClient _httpClient;
        CancellationTokenSource _cancellationTokenSource;
        IRouteRepository _routeRepository;
        public MapServices(IRouteRepository routeRepository) 
        {
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
            _routeRepository = routeRepository;
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
                //if (location != null)
                //{
                //    var coordinates = new MPoint(location.Latitude, location.Longitude);
                //}
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
            var response = await _httpClient.GetAsync($"{url}?q={input}&bounded=1&format=json&viewbox={minX},{minY},{maxX},{maxY}", _cancellationTokenSource.Token);

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

        public async Task GetDirectionsAsync(double origin, double destination)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            var orsRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openrouteservice.org/v2/directions/driving-car/geojson");
            //OpenRouteService Directions API Token
            orsRequest.Headers.Add("Authorization", "Bearer 5b3ce3597851110001cf6248d82ffd7c0abb468cbe0cd0bf61653d82");
            var content = new StringContent("{\"coordinates\":[[123.888767,10.298491],[123.906199,10.293772]],\"alternative_routes\":{\"target_count\":3,\"weight_factor\":1.4,\"share_factor\":0.6},\"extra_info\":[\"osmid\"]}", null, "application/json");
            orsRequest.Content = content;
            //Get route from origin to destination
            var response = await _httpClient.SendAsync(orsRequest);
            response.EnsureSuccessStatusCode();

            ORSDirectionsDTO responseData = await response.Content.ReadFromJsonAsync<ORSDirectionsDTO>();

            //loop each coordinate
            foreach(var feature in responseData.features)
            {
                List<Route> puvRoutes = [];
                List<List<OSMCoordinate>> puvGeom = [];
                List<Coordinate> routeCoordinates = [];
                List<Coordinate> puvRouteCoordinates = [];
                //get each coordinate
                foreach(var coordinate in feature.geometry.coordinates)
                {
                    //search for osm object connected to the coordinate
                    string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={Uri.EscapeDataString(coordinate[1].ToString())}&lon={Uri.EscapeDataString(coordinate[0].ToString())}";
                    var nominatimRequest = new HttpRequestMessage(HttpMethod.Get, url);
                    var nominatimResponse = await _httpClient.SendAsync(nominatimRequest);
                    nominatimResponse.EnsureSuccessStatusCode();
                    var nominatimResponseData = await nominatimResponse.Content.ReadFromJsonAsync<LocationDTO>();
                    if( nominatimResponseData != null)
                    {
                        if(nominatimResponseData.osm_type == "way")
                        {
                            string overpassUrl = "https://overpass-api.de/api/interpreter";
                            string query = $@"
                                [out:json];
                                way({nominatimResponseData.osm_id});
                                rel(bw);
                                out geom;";
                            //get the available PUV routes in the O-D path
                            var overpassResponse = await _httpClient.GetAsync($"{ overpassUrl}?data={Uri.EscapeDataString(query)}");
                            if (overpassResponse.IsSuccessStatusCode)
                            {
                                OSMDataDTO overpassResponseData = await overpassResponse.Content.ReadFromJsonAsync<OSMDataDTO>();
                                foreach(var element in overpassResponseData.elements)
                                {
                                    puvRoutes.Add(new Route
                                    {
                                        Osm_Id = element.id,
                                        Code = element.tags.TryGetValue("ref", out string code) ? code : "No Code",
                                        Name = element.tags.TryGetValue("name", out string name) ? name : "No Name",
                                    });
                                }
                            }
                            //if (await _routeRepository.CountRoutesAsync() != 0)
                            //{
                            //    var routes = await _routeRepository.GetRoutesByWayId(nominatimResponseData.osm_id);
                            //}
                        }
                    }
                    //add coordinate to the final route
                    routeCoordinates.Add(coordinate);
                }
            }
            Console.WriteLine(responseData);
            Debug.WriteLine(responseData);
            return;
        }
        public async Task<Map> addLineString(Map map)
        {
            var lineStringLayer = CreateLineStringLayer(CreateLineStringStyle());
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
        public string StreetListToWkt(List<Coordinate> coordinates)
        {
            if (coordinates == null || coordinates.Count == 0)
            {
                return string.Empty;
            }

            // Construct the LINESTRING WKT string
            StringBuilder wktBuilder = new StringBuilder();
            wktBuilder.Append("LINESTRING (");

            foreach (Coordinate coord in coordinates)
            {
                wktBuilder.Append(coord.X).Append(" ").Append(coord.Y).Append(", ");
            }

            // Remove the trailing comma and space
            wktBuilder.Length -= 2;
            wktBuilder.Append(")");

            return wktBuilder.ToString();
        }


        public static ILayer CreateLineStringLayer(IStyle style = null)
        {
            var lineString = (LineString)new WKTReader().Read(WKTGr5);
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
