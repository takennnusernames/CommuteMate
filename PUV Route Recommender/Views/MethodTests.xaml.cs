using CommuteMate.Interfaces;
using Mapsui.UI.Maui;
using NetTopologySuite.Geometries;
using NetTopologySuite.LinearReferencing;
using NetTopologySuite.Operation.Distance;
using QuickGraph;
using Topten.RichTextKit.Editor;
using Point = NetTopologySuite.Geometries.Point;

namespace CommuteMate.Views;

public partial class MethodTests : ContentPage
{
	private readonly IMapServices _mapServices;
    private readonly IStreetService _streetService;
    private readonly IOverpassApiServices _overpassApiServices;
    private readonly IRouteService _routeServices;
	public MethodTests(IMapServices mapService, IStreetService streetService, IOverpassApiServices overpassApiServices, IRouteService routeServices)
    {
        _mapServices = mapService;
        _streetService = streetService;
        _overpassApiServices = overpassApiServices;
        _routeServices = routeServices;
        InitializeComponent();
	}

    private async void Button_Pressed(object sender, EventArgs e)
    {
		Coordinate origin = new Coordinate(123.89539, 10.31035);
		Coordinate destination = new Coordinate(123.90006, 10.30421);
        try
        {
            var map = await _mapServices.CreateMapAsync();
            var directions = await _mapServices.GetDirectionsAsync(origin, destination);
            //(Route, Path, Fare, Distance)
            List<List<PathData>> options = [];
            foreach (var feature in directions.features)
            {
                options.Add(await _mapServices.GetOptions(feature));
            }
            
            mapControlTest.Map = map;
        }
        catch(Exception ex) 
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok");
        }
    }

    private async void Button_Pressed_Sequence(object sender, EventArgs e)
    {
        try
        {
            //var map = await _mapServices.CreateMapAsync();
            //var streets = await _overpassApiServices.RetrieveStreetWithCoordinatesAsync(3199499);
            //var graph = await _routeServices.StreetToGraph(streets, 3199499);
            ////var shortestPath = _routeServices.GetShortetstPath(graph, new Coordinate(123.8953882, 10.3102911), new Coordinate(123.9000549, 10.3039991));

            //// Group edges by connected components
            //var components = new Dictionary<Coordinate, List<Edge<Coordinate>>>();
            //foreach (var edge in graph.Edges)
            //{
            //    if (!components.TryGetValue(edge.Source, out var list))
            //    {
            //        list = new List<Edge<Coordinate>>();
            //        components.Add(edge.Source, list);
            //    }
            //    list.Add(edge);
            //}

            //// Draw each connected component separately
            //foreach (var component in components.Values)
            //{
            //    var coordinates = new List<Coordinate>();
            //    foreach (var edge in component)
            //    {
            //        coordinates.Add(edge.Source);
            //        coordinates.Add(edge.Target);
            //    }
            //    string lineString = await _streetService.StreetListToWkt(coordinates);
            //    await _mapServices.addLineString(map, lineString);
            //}

            //mapControlTest.Map = map;
            
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok");
        }
        
    }
    
}