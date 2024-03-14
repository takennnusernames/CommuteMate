using PUV_Route_Recommender.Views;

namespace PUV_Route_Recommender
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(NavigatingPage), typeof(NavigatingPage));
            Routing.RegisterRoute(nameof(InfoPage), typeof(InfoPage));
            Routing.RegisterRoute(nameof(VehicleInfoPage), typeof(VehicleInfoPage));
            Routing.RegisterRoute(nameof(RoutesView), typeof(RoutesView));
            Routing.RegisterRoute(nameof(RoutesInfoPage), typeof(RoutesInfoPage));
        }
    }
}