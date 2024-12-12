using CommuteMate.Views;

namespace CommuteMate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(NavigatingPage), typeof(NavigatingPage));
            Routing.RegisterRoute(nameof(VehicleInfoPage), typeof(VehicleInfoPage));
            Routing.RegisterRoute(nameof(RoutesView), typeof(RoutesView));
            Routing.RegisterRoute(nameof(RouteListView), typeof(RouteListView));
            Routing.RegisterRoute(nameof(RouteDetailsView), typeof(RouteDetailsView));
            Routing.RegisterRoute(nameof(SurveyPage), typeof(SurveyPage));
            Routing.RegisterRoute(nameof(MapView), typeof(MapView));
            Routing.RegisterRoute(nameof(OfflinePathMapView), typeof(OfflinePathMapView));
        }
    }
}