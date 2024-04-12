using PUV_Route_Recommender.Interfaces;

namespace PUV_Route_Recommender.ViewModels
{
    [QueryProperty(nameof(Route), "Route")]
    public partial class RouteInfoViewModel : BaseViewModel
    {
        private readonly IRouteStreetService _routeStreetService;
        public RouteInfoViewModel(IRouteStreetService routeStreetService) 
        {
            Title = "Route Details";
            _routeStreetService = routeStreetService;
        }
        [ObservableProperty]
        private RouteInfo route;


    }

}
