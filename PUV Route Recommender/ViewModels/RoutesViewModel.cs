using PUV_Route_Recommender.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.ViewModels
{
    public partial class RoutesViewModel : BaseViewModel
    {
        public ObservableCollection<Route> Routes { get; } = new();
        IOverpassApiServices overpassApiServices;
        IConnectivity connectivity;
        IRouteService routeService;
        public RoutesViewModel(IOverpassApiServices overpassApiServices, IConnectivity connectivity, IRouteService routeService) 
        {
            Title = "Route List";
            this.overpassApiServices = overpassApiServices;
            this.connectivity = connectivity;
            this.routeService = routeService;
        }

        [RelayCommand]
        async Task GetRoutesAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                var routes = await routeService.GetRoutesAsync();

                if(Routes.Count != 0)
                    Routes.Clear();

                foreach(var route in routes)
                    Routes.Add(route);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get routes: {ex.Message}");
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
