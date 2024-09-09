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
    protected override void OnAppearing()
    {
        base.OnAppearing();
        CreateGoogleMapAsync(testMap);
        foreach(var street in _streets)
        {
            var line = (LineString)new WKTReader().Read(street);
            AddGooglePolyline(line, testMap);
        }
    }

}