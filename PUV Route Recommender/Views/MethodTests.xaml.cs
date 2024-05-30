using BruTile.Wmts.Generated;
using CommuteMate.Interfaces;
using CommuteMate.Views.SlideUpSheets;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Distance;
using The49.Maui.BottomSheet;
using Topten.RichTextKit.Editor;
using GoogleMap = Microsoft.Maui.Controls.Maps.Map;
using Point = NetTopologySuite.Geometries.Point;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;

namespace CommuteMate.Views;

public partial class MethodTests : ContentPage
{
	private readonly IMapServices _mapServices;
    private readonly IStreetService _streetService;
    private readonly IRouteService _routeServices;
    public MethodTests(IMapServices mapService, IStreetService streetService, IRouteService routeServices)
    {
        _mapServices = mapService;
        _streetService = streetService;
        _routeServices = routeServices;
        InitializeComponent();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
    private void BottomSheet_Dismissed(object sender, DismissOrigin e)
    {
        TestButton.IsVisible = true;
    }
    
}