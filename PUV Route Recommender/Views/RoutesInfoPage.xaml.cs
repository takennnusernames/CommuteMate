using PUV_Route_Recommender.Models;
using PUV_Route_Recommender.Repositories;

namespace PUV_Route_Recommender.Views;

[QueryProperty(nameof(Osm_Id), "Id")]
public partial class RoutesInfoPage : ContentPage
{
    private Route _route;
    private readonly RouteRepository _routeRepository;
    public RoutesInfoPage(RouteRepository routeRepository)
	{
		InitializeComponent();
        _routeRepository = routeRepository;
    }
    public string Osm_Id
    {
        set
        {
            _route = _routeRepository.GetRouteById(int.Parse(value));
            lblName.Text = _route.Code;
            InitializeAsync().Wait();
        }
        get
        {
            return _route?.Osm_Id.ToString();
        }
    }
    public async Task InitializeAsync()
    {
        List<string> streets = await _routeRepository.GetRouteStreets(int.Parse(Osm_Id));
        street_list.ItemsSource = streets;
    }
    
}