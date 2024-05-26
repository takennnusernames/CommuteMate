using BruTile.Wmts.Generated;
using CommuteMate.Interfaces;
using CommuteMate.Views.SlideUpSheets;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Distance;
using QuickGraph;
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
    private readonly IOverpassApiServices _overpassApiServices;
    private readonly IRouteService _routeServices;

    private readonly TestSheet testSheet;
    //readonly RoutePathDetails details;
    public MethodTests(IMapServices mapService, IStreetService streetService, IOverpassApiServices overpassApiServices, IRouteService routeServices)
    {
        _mapServices = mapService;
        _streetService = streetService;
        _overpassApiServices = overpassApiServices;
        _routeServices = routeServices;
        testSheet = new TestSheet(); 
        InitializeComponent();
        OpenPeekableSheet();
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
    private void BottomSheet_Dismissed(object sender, DismissOrigin e)
    {
        TestButton.IsVisible = true;
    }
    private void Button_Pressed(object sender, EventArgs e)
    {
        OpenPeekableSheet();
    }
    private void OpenPeekableSheet()
    {
        testSheet.Detents = new DetentsCollection()
        {
            new FullscreenDetent(),
            new ContentDetent()
        };
        testSheet.ShowAsync();
        TestButton.IsVisible = false;
    }
    private async void Button_Pressed_Sequence(object sender, EventArgs e)
    {
        await testSheet.ShowAsync();
        testSheet.IsVisible = true;
        TestButton.IsVisible = false;    
    }
    
}