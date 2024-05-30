using CommuteMate.Interfaces;
using NetTopologySuite.Geometries;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using System.Net.Http.Json;
using CommuteMate.DTO;
using Coordinate = NetTopologySuite.Geometries.Coordinate;
using Point = NetTopologySuite.Geometries.Point;
using Exception = System.Exception;
using Microsoft.Maui.Maps;
using GoogleMap = Microsoft.Maui.Controls.Maps.Map;
using Microsoft.Maui.Controls.Maps;
using Colors = Microsoft.Maui.Graphics.Colors;
using Mapsui.UI.Maui;

namespace CommuteMate.Services
{
    public class MapServices : IMapServices
    {
        readonly HttpClient _httpClient;
        CancellationTokenSource _cancellationTokenSource;
        IGeolocation _geolocation;
        public MapServices(IGeolocation geolocation) 
        {
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
            _geolocation = geolocation;
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
            map.MoveToRegion(mapSpan);
            return Task.FromResult(pin);
        }

        public Task<CustomPin> AddCustomPin(LocationDetails location, GoogleMap map)
        {
            var customPin = new CustomPin
            {
                Location = new Location
                {
                    Latitude = location.Coordinate.Y,
                    Longitude = location.Coordinate.X
                },
                Label = location.Name,
                Address = location.Coordinate.X.ToString() + ", " + location.Coordinate.Y.ToString(),
                ImageSource = "dotnet_bot" // Name of the image file without the extension
            };

            MapSpan mapSpan = new MapSpan(customPin.Location, 0.01, 0.01);
            map.Pins.Add(pin);
            map.MoveToRegion(mapSpan);
            return Task.FromResult(customPin);
        }
        public async Task<Pin> AddGoogleMarker(Location location, GoogleMap map, string label, string action)
        {
            var pin = new Pin
            {
                Type = PinType.Generic,
                Location = location,
                Label = label,
                Address = action
            };
            map.Pins.Add(pin);
            return await Task.FromResult(pin);
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
            
            var color = Colors.Coral;
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

        public async Task<Polyline> AddGooglePolyline(Geometry geometry, GoogleMap map, string action, Microsoft.Maui.Graphics.Color color)
        {
            List<Location> locations = [];
            if (geometry.GeometryType.Equals("MultiLineString"))
            {
                var multiLineString = (MultiLineString)geometry;

                foreach (var lineString in multiLineString.Geometries)
                {
                    if (lineString.GeometryType == "Point")
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
            var polyline = new Polyline
            {
                StrokeColor = color,
                StrokeWidth = 9
            };

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
