using CommuteMate.Views;

namespace CommuteMate
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
            Routing.RegisterRoute(nameof(MethodTests), typeof(MethodTests));
        }
    }
}