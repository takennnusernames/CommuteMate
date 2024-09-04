using CommuteMate.Services;
using Microsoft.Maui.Maps;
using NetTopologySuite.Geometries;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using GoogleMap = Microsoft.Maui.Controls.Maps.Map;
using Microsoft.Maui.Controls.Maps;
using NetTopologySuite.IO;
using Microsoft.Maui.Controls.Shapes;

namespace CommuteMate.Views;

[QueryProperty(nameof(Streets), "Streets")]
public partial class MapView : ContentPage
{
    readonly MapServices _mapServices;
    public List<string> _streets { get; set; }
    public string Streets
    {
        get => null; // This property is only used to receive the serialized string.
        set
        {
            // Deserialize the string into a List<string>
            _streets = JsonSerializer.Deserialize<List<string>>(value);
        }
    }
    public MapView()
    {
        InitializeComponent();
    }
    Task CreateGoogleMapAsync(GoogleMap map)
    {
        Location location = new Location(10.3157, 123.8854);
        MapSpan mapSpan = new MapSpan(location, 0.05, 0.05);
        map.MoveToRegion(mapSpan);

        return Task.FromResult(map);
    }

    Task AddGooglePolyline(LineString geometry, GoogleMap map)
    {
        List<Location> locations = [];

        locations.AddRange(ConvertLineStringToLocations((LineString)geometry));

        var polyline = new Microsoft.Maui.Controls.Maps.Polyline();

        polyline.StrokeColor = Colors.Green;
        polyline.StrokeWidth = 6;

        foreach (var position in locations)
        {
            polyline.Geopath.Add(position);
        }
        map.MapElements.Add(polyline);

        return Task.FromResult(map);
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

    async Task<List<Coordinate>> WktToLineString(string wktString)
    {
        await Task.Delay(0);


        // Find the index of the opening and closing parentheses
        int startIndex = wktString.IndexOf('(');
        int endIndex = wktString.LastIndexOf(')');

        // Extract the substring between the parentheses
        string coordinatesString = wktString.Substring(startIndex + 1, endIndex - startIndex - 1);

        // Split coordinates string into individual coordinates
        string[] coordinateStrings = coordinatesString.Split(',');

        List<Coordinate> coordinates = new List<Coordinate>();

        foreach (var coordinateString in coordinateStrings)
        {
            // Split coordinate string into X and Y
            string[] parts = coordinateString.Trim().Split(' ');

            // Parse X and Y values
            if (parts.Length == 2 && double.TryParse(parts[0], out double x) && double.TryParse(parts[1], out double y))
            {
                coordinates.Add(new Coordinate { X = x, Y = y });
            }
            else
            {
                // Handle invalid coordinate format
                throw new FormatException("Invalid coordinate format in WKT string.");
            }
        }

        return coordinates;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CreateGoogleMapAsync(testMap);
        List<LineString> lines = new List<LineString>();
        foreach(var street in _streets)
        {
            var line = (LineString)new WKTReader().Read(street);
            AddGooglePolyline(line, testMap);
        }
    }

}