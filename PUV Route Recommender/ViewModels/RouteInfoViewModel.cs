using CommuteMate.Interfaces;

namespace CommuteMate.ViewModels
{
    [QueryProperty(nameof(Route), "Route")]
    public partial class RouteInfoViewModel : BaseViewModel
    {
        public RouteInfoViewModel() 
        {
            Title = "Route Details";
        }
        [ObservableProperty]
        private RouteInfo route;


    }

}
