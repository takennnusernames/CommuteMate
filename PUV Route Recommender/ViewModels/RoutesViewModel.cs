using CommuteMate.ApiClient;
using ApiRoute = CommuteMate.ApiClient.Models.ApiModels.Route;
using CommuteMate.Interfaces;
using CommuteMate.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.ViewModels
{
    public partial class RoutesViewModel : BaseViewModel
    {  
        readonly IOverpassApiServices _overpassApiServices;
        readonly IConnectivity _connectivity;
        readonly IRouteService _routeService;
        readonly ICommuteMateApiService _commuteMateApiService;
        readonly CommuteMateApiClientService _apiClient;
        public RoutesViewModel(
            IOverpassApiServices overpassApiServices, 
            IConnectivity connectivity,
            IRouteService routeService,
            ICommuteMateApiService commuteMateApiService,
            CommuteMateApiClientService apiClientService) 
        {
            Title = "Route List";
            _overpassApiServices = overpassApiServices;
            _connectivity = connectivity;
            _routeService = routeService;
            _apiClient = apiClientService;
            _commuteMateApiService = commuteMateApiService;
        }

        //properties
        public ObservableCollection<Route> Routes { get; } = [];
        public Button getRoutesButton { get; set; }
        //commands
        [RelayCommand]
        async Task GetRoutesAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("No connectivity!",
                        $"Please check internet and try again.", "OK");
                    return;
                }
                var routes = await _commuteMateApiService.GetRoutes();

                if(Routes.Count != 0)
                    Routes.Clear();

                if (routes.Count == 0)
                {
                    throw new Exception("empty route");
                }

                foreach (var route in routes)
                    Routes.Add(route);

                getRoutesButton.IsEnabled = false;
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

        [RelayCommand]
        async Task GoToDetails(Route route)
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("No connectivity!",
                        $"Please check internet and try again.", "OK");
                    return;
                }
                var streets = await _commuteMateApiService.GetRouteStreets(route.Osm_Id) ?? throw new Exception("streets is null");
                var streetNames = streets.GroupBy(s => s.Name).Select(g => g.Key).ToList();
                RouteInfo routeInfo = new RouteInfo
                {
                    RouteName = route.Name,
                    StreetNames = streetNames
                };
                await Shell.Current.GoToAsync($"{nameof(RoutesInfoPage)}", true,
                    new Dictionary<string, object>
                    {
                        {"Route", routeInfo}
                    });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error! Unable to retrieve information", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
            if (route == null) return;
            
        }
    }
}
