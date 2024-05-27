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
        public RoutesViewModel(
            IOverpassApiServices overpassApiServices, 
            IConnectivity connectivity,
            IRouteService routeService,
            ICommuteMateApiService commuteMateApiService) 
        {
            Title = "Route List";
            _overpassApiServices = overpassApiServices;
            _connectivity = connectivity;
            _routeService = routeService;
            _commuteMateApiService = commuteMateApiService;
            _isStreetFrameVisible = false;
        }
        //properties
        public ObservableCollection<Route> Routes { get; } = [];

        public ObservableCollection<string> Streets { get; } = [];
        public Button getRoutesButton { get; set; }
        private bool _isStreetFrameVisible;
        public bool IsStreetFrameVisible
        {
            get => _isStreetFrameVisible;
            set
            {
                _isStreetFrameVisible = value;
                OnPropertyChanged();
            }
        }
        //commands
        [RelayCommand]
        async Task ShowRoutesAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                var routes = await _routeService.GetAllRoutesAsync();

                if (Routes.Count != 0)
                    Routes.Clear();

                if (routes.Count == 0 || routes is null)
                {
                    throw new Exception("empty route");
                }

                foreach (var route in routes)
                {
                    var parts = route.Name.Split(new[] { ':' }, 2);
                    var name = parts.Length > 1 ? parts[1] : route.Name;
                    route.Name = name;
                    Routes.Add(route);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get routes: {ex.Message}");
                await Shell.Current.DisplayAlert("Alert!", "Press Retrieve Routes button to Load Routes", "OK");

            }
            finally
            {
                IsBusy = false;
            }
        }
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
        public async Task<List<string>> GoToDetails(Route route)
        {
            if (IsBusy)
                return null;
            try
            {
                IsBusy = true;
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("No connectivity!",
                        $"Please check internet and try again.", "OK");
                    return null;
                }
                var streets = await _commuteMateApiService.GetRouteStreets(route.Osm_Id) ?? throw new Exception("streets is null");
                var streetNames = streets.GroupBy(s => s.Name).Select(g => g.Key).ToList();

                
                return streetNames;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error! Unable to retrieve information", ex.Message, "OK");
                return null;
            }
            finally
            {
                IsBusy = false;
            }
            
        }
    }
}
