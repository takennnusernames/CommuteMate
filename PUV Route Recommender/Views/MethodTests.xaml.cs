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
		Coordinate origin = new Coordinate(123.90739229999998,10.3304499);
		Coordinate destination = new Coordinate(123.8806068, 10.2817312);
        try
        {
            var map = await _mapServices.CreateMapAsync();
            var directions = await _mapServices.GetDirectionsAsync(origin, destination);
            //(Route, Path, Fare, Distance)
            var feature = directions.features.FirstOrDefault();

            var streetList = await _overpassApiServices.GeometryToStreetListAsync(feature.geometry);
            var streets = streetList.Select(tuple => tuple.Item1).GroupBy(s => s.Name).Select(g => g.First()).ToList();
            List<Coordinate> coordinates = [];
            foreach(var street in streets)
            {
                coordinates.Add(streetList.Find(s => s.Item1 == street).Item2);
            }
            string lineString = await _streetService.StreetListToWkt(coordinates);
            await _mapServices.addLineString(map, lineString, "dotted");
            
            //var sequencedStreet = await _streetService.StreetSequence(streetList);
            //foreach (var street in sequencedStreet)
            //{
            //    Console.WriteLine($"{street.Osm_Id}");
            //}

            //var graph =  _routeServices.StreetToGraph(streets);
            //var components = new Dictionary<Coordinate, List<Edge<Coordinate>>>();

            //var coordinates = new List<Coordinate>();
            //foreach (var edge in graph.Edges)
            //{
            //    coordinates.Add(edge.Source);
            //    coordinates.Add(edge.Target);
            //}

            //string lineString = await _streetService.StreetListToWkt(coordinates);

            //await _mapServices.addLineString(map, lineString, "dotted");
            //var options = await _mapServices.GetOptions(feature);
            //var option = options.First();

            //await _mapServices.addPath(map, option.walkingPath, "dotted");
            //await _mapServices.addPath(map, option.puvShortestPaths, "straight");

            mapControlTest.Map = map;


            //var routesQueues = await _mapServices.GetRoutesQueue(streets);
            //Console.WriteLine($"{routesQueues.ToList()}");
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
            var location = await _mapServices.GetCurrentLocationAsync();
            var map = await _mapServices.CreateMapAsync();
            var streets = await _overpassApiServices.RetrieveStreetWithCoordinatesAsync(3199499);
            var graph = await _routeServices.StreetToGraph(streets, 3199499);
            var shortestPath = _routeServices.GetShortetstPath(graph, new Coordinate(123.8953882, 10.3102911), new Coordinate(123.9000549, 10.3039991));

            // Group edges by connected components
            var components = new Dictionary<Coordinate, List<Edge<Coordinate>>>();
            foreach (var edge in shortestPath)
            {
                if (!components.TryGetValue(edge.Source, out var list))
                {
                    list = new List<Edge<Coordinate>>();
                    components.Add(edge.Source, list);
                }
                list.Add(edge);
            }

            // Draw each connected component separately
            foreach (var component in components.Values)
            {
                var coordinates = new List<Coordinate>();
                foreach (var edge in component)
                {
                    coordinates.Add(edge.Source);
                    coordinates.Add(edge.Target);
                }
                string lineString = await _streetService.StreetListToWkt(coordinates);
                await _mapServices.addLineString(map, lineString, "test");
            }

            mapControlTest.Map = map;

        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok");
        }
        
    }
    
}