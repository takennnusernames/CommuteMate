using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using Map = Mapsui.Map;
using Microsoft.Maui.ApplicationModel;

namespace PUV_Route_Recommender.Views;

public partial class NavigatingPage : ContentPage
{
	public NavigatingPage()
	{
		InitializeComponent();
        InitializeMap();
    }
    private void InitializeMap()
    {
        //Map map = mapControl.Map;
        //var openStreetMapLayer = OpenStreetMap.CreateTileLayer();
        //map.Layers.Add(openStreetMapLayer);

        //map.Navigator.ZoomTo(map.Navigator.Resolutions[18], new MPoint(123.8854, 10.3157));

        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        // Get the lon lat coordinates from somewhere (Mapsui can not help you there)
        var centerOfLondonOntario = new MPoint(-81.2497, 42.9837);
        // OSM uses spherical mercator coordinates. So transform the lon lat coordinates to spherical mercator
        var sphericalMercatorCoordinate = SphericalMercator.FromLonLat(centerOfLondonOntario.X, centerOfLondonOntario.Y).ToMPoint();
        // Set the center of the viewport to the coordinate. The UI will refresh automatically
        // Additionally you might want to set the resolution, this could depend on your specific purpose
        map.Navigator.CenterOnAndZoomTo(sphericalMercatorCoordinate, map.Navigator.Resolutions[9]);

        mapControl.Map = map;

    }
}