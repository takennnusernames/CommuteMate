using PUV_Route_Recommender.Interfaces;
using PUV_Route_Recommender.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.ViewModels
{
    public partial class RoutesViewModel : BaseViewModel
    {  
        readonly IOverpassApiServices _overpassApiServices;
        readonly IConnectivity _connectivity;
        readonly IRouteService _routeService;
        readonly IRouteStreetService _routeStreetService;
        public RoutesViewModel(
            IOverpassApiServices overpassApiServices, 
            IConnectivity connectivity,
            IRouteService routeService,
            IRouteStreetService routeStreetService) 
        {
            Title = "Route List";
            _overpassApiServices = overpassApiServices;
            _connectivity = connectivity;
            _routeService = routeService;
            _routeStreetService = routeStreetService;
        }

        //properties
        public ObservableCollection<Route> Routes { get; } = [];
        //commands
        [RelayCommand]
        async Task GetRoutesAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                var routes = await _routeService.GetAllRoutesAsync();

                if(Routes.Count != 0)
                    Routes.Clear();

                if (routes.Count == 0)
                {
                    throw new Exception("empty route");
                }

                foreach (var route in routes)
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

        [RelayCommand]
        async Task GetDataAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                await _overpassApiServices.RetrieveOverpassRoutesAsync();
                await Shell.Current.DisplayAlert("Success!", "Data Retrieved", "OK");
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
                var streets = await _routeStreetService.GetRouteStreetsAsync(route.Id);
                if(streets.Count is 0)
                {
                    await _overpassApiServices.RetrieveOverpassRouteStreetsAsync(route.Osm_Id, route.Id);
                    streets = await _routeStreetService.GetRouteStreetsAsync(route.Id);
                }
                List<string> streetNames = new List<string>();
                foreach (var street in streets)
                {
                    if (!streetNames.Contains(street.Name))
                    {
                        streetNames.Add(street.Name);
                    }
                }
                RouteInfo routeInfo = new RouteInfo
                {
                    Route = route,
                    Street = streets,
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
