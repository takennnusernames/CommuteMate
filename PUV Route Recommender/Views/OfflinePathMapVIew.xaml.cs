using Microsoft.Maui.Maps;
using NetTopologySuite.Geometries;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using GoogleMap = Microsoft.Maui.Controls.Maps.Map;
using NetTopologySuite.IO;
using static Mapsui.Rendering.Skia.Functions.ClippingFunctions;
using Point = NetTopologySuite.Geometries.Point;
using Microsoft.Maui.Controls.Maps;
using MapSpan = Microsoft.Maui.Maps.MapSpan;
using Polyline = Microsoft.Maui.Controls.Maps.Polyline;
using CommuteMate.Views.SlideUpSheets;

namespace CommuteMate.Views;
[QueryProperty(nameof(Path), "Path")]
public partial class OfflinePathMapView : ContentPage
{
    public OfflinePath _path { get; set; }
    public OfflinePath Path
    {
        get => null; // This property is only used to receive the serialized string.
        set
        {
            // Deserialize the string into a List<string>
            _path = value;
            Title = _path?.PathName;
            SlideUpCard = new OfflineSlideUpCard(_path);
        }
    }
    public OfflinePathMapView()
	{
        InitializeComponent();
    }


    public OfflineSlideUpCard SlideUpCard { get; private set; }

    public async void ShowSlideUpButton()
    {
        ShowDetailsButton.IsVisible = true;
        await ShowDetailsButton.FadeTo(1, 500);
        ShowDetailsButton.Scale = 0.1;
        await ShowDetailsButton.ScaleTo(1, 250, Easing.CubicIn);
    }
    Task CreateGoogleMapAsync(GoogleMap map)
    {
        Location location = new Location(10.3157, 123.8854);
        MapSpan mapSpan = new MapSpan(location, 0.05, 0.05);
        map.MoveToRegion(mapSpan);

        return Task.FromResult(map);
    }

    void AddGooglePolyline(Geometry geometry, GoogleMap map, Microsoft.Maui.Graphics.Color color)
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
    }
    void AddCustomPin(Geometry geometry, GoogleMap map, string label, string action)
    {
        var customPin = new CustomPin
        {
            Location = new Location
            {
                Latitude = geometry.Coordinate.X,
                Longitude = geometry.Coordinate.Y
            },
            Label = action,
            Address = label,
            ImageSource = "still_icon"
        };
        map.Pins.Add(customPin);
    }

    void AddGooglePin(Point location, string name, GoogleMap map)
    {
        var pin = new Pin
        {
            Type = PinType.Place,
            Location = new Location
            {
                Latitude = location.Coordinate.Y,
                Longitude = location.Coordinate.X
            },
            Label = name,
            Address = location.Coordinate.X.ToString() + ", " + location.Coordinate.Y.ToString()
        };

        map.Pins.Add(pin);
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
        CreateGoogleMapAsync(offlineMap);
        ShowPath();
        ShowSlideUpButton();

    }

    void ShowPath()
    {
        Queue<Color> colorQueue = new Queue<Color>();
        colorQueue.Enqueue(Colors.Orange);
        colorQueue.Enqueue(Colors.Blue);
        colorQueue.Enqueue(Colors.Red);
        colorQueue.Enqueue(Colors.Green);
        colorQueue.Enqueue(Colors.Yellow);
        colorQueue.Enqueue(Colors.Gray);

        var origin = (Point)new WKTReader().Read(_path.OriginPoint);
        AddGooglePin(origin, _path.Origin, offlineMap);

        var destination = (Point)new WKTReader().Read(_path.DestinationPoint);
        AddGooglePin(destination, _path.Destination, offlineMap);

        foreach (var pathStep in _path.PathSteps)
        {
            var step = pathStep.Step;
            var line = (Geometry)new WKTReader().Read(step.GeometryWkt);
            if (step.Action.Contains("Walk"))
            {
                AddGooglePolyline(line, offlineMap, Colors.Black);
            }
            else if (step.Action.Contains("Ride"))
            {
                var color = colorQueue.Dequeue();
                AddGooglePolyline(line, offlineMap, color);
            }
            else
            {
                AddCustomPin(line, offlineMap, step.Action, step.Instruction);
            }
        }

        MapSpan mapSpan = new MapSpan(new Microsoft.Maui.Devices.Sensors.Location(origin.Coordinate.Y, origin.Coordinate.X), 0.03, 0.03);

        offlineMap.MoveToRegion(mapSpan);
    }
    private async void ImageButton_Clicked(object sender, EventArgs e)
    {
        await SlideUpCard.ShowAsync();
    }

}