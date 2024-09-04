using CommuteMate.ApiClient;
using ApiRoute = CommuteMate.ApiClient.Models.ApiModels.Route;
using CommuteMate.Interfaces;
using CommuteMate.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using CommuteMate.DTO;

namespace CommuteMate.ViewModels
{
    public partial class RoutesViewModel : BaseViewModel
    {
        readonly IConnectivity _connectivity;
        readonly IRouteService _routeService;
        readonly IStreetService _streetService;
        readonly ICommuteMateApiService _commuteMateApiService;
        readonly IMapServices _mapServices;
        public RoutesViewModel(
            IConnectivity connectivity,
            IRouteService routeService,
            IStreetService streetService,
            ICommuteMateApiService commuteMateApiService,
            IMapServices mapServices)
        {
            Title = "Route List";
            _connectivity = connectivity;
            _routeService = routeService;
            _streetService = streetService;
            _commuteMateApiService = commuteMateApiService;
            _mapServices = mapServices;
        }
        //properties
        private ObservableCollection<RouteView> _allRoutes = [];

        private ObservableCollection<RouteView> _filteredRoutes = [];

        public ObservableCollection<RouteView> Routes
        {
            get => _filteredRoutes;
            set
            {
                _filteredRoutes = value;
                OnPropertyChanged();
            }
        }

        private int _pageSize = 10;
        private int _currentPage = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public int CurrentPage
        {
            get { return _currentPage; }
            set { _currentPage = value; }
        }

        public ObservableCollection<Route> FilteredRoutes { get; } = [];
        public ObservableCollection<RouteView> SearchResults { get; } = [];
        public ObservableCollection<Street> Streets { get; } = [];
        [ObservableProperty]
        string searchInput;
        public Button getRoutesButton { get; set; }
        public SearchBar RoutesSearchBar { get; set; }
        public Button CancelSearchButton { get; set; }

        private List<string> _streetNames = [];
        private List<string> _streetGeometries = [];
       

        public ObservableCollection<List<string>> StreetList = new();
        //commands
        [RelayCommand]
        async Task ShowRoutesAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                List<Route> routes = [];

                if (_allRoutes.Count > 0)
                    return;

                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("You are offline!",
                        $"Only the downloaded routes will be shown", "OK");

                    routes = await _routeService.GetOfflineRoutesAsync();
                }

                else
                    routes = await _routeService.GetAllRoutesAsync();


                if (_allRoutes.Count != 0)
                    _allRoutes.Clear();

                if (routes.Count == 0)
                {
                    throw new Exception("empty streets");
                }

                foreach (var route in routes)
                {
                    var parts = route.Name.Split(new[] { ':' }, 2);
                    var name = parts.Length > 1 ? parts[1] : route.Name;
                    //route.Name = name;
                    //Routes.Add(route);

                    RouteView newRoute = new RouteView
                    {
                        Code = route.Code,
                        Osm_Id = route.Osm_Id,
                        Name = name,
                        IsStreetFrameVisible = false
                    };
                    Routes.Add(newRoute);
                    _allRoutes.Add(newRoute);
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

                if (_allRoutes.Count != 0)
                    _allRoutes.Clear();

                if (routes.Count == 0)
                {
                    throw new Exception("empty streets");
                }

                foreach (var route in routes)
                {
                    var parts = route.Name.Split(new[] { ':' }, 2);
                    var name = parts.Length > 1 ? parts[1] : route.Name;
                    //route.Name = name;
                    //Routes.Add(route);
                    RouteView newRoute = new RouteView
                    {
                        Code = route.Code,
                        Osm_Id = route.Osm_Id,
                        Name = name,
                        IsStreetFrameVisible = false
                    };
                    Routes.Add(newRoute);
                    _allRoutes.Add(newRoute);
                }

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
        async Task GetStreets(RouteView route)
        {
            if (IsBusy)
                return;
            try
            {
                if (route.IsStreetFrameVisible)
                {
                    route.IsStreetFrameVisible = false;
                    return;
                }

                IsBusy = true;
                List<Street> streets = [];
                if(route.IsDownloaded)
                    streets = await _streetService.GetStreetByRouteIdAsync(route.Osm_Id);
                else
                {
                    if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        await Shell.Current.DisplayAlert("No connectivity!",
                            $"Please check internet and try again.", "OK");
                        return;
                    }

                    streets = await _commuteMateApiService.GetRouteStreets(route.Osm_Id) ?? throw new Exception("streets is null");
                    string count = streets.Count.ToString();
                    await Shell.Current.DisplayAlert("Test", count, "Ok");
                }

                route.IsStreetFrameVisible = true;
                _streetNames.Clear();
                _streetNames = streets.GroupBy(s => s.Name).Select(g => g.Key).ToList();
                _streetNames.Add("test");
                _streetGeometries.Clear();
                _streetGeometries = streets.GroupBy(s => s.GeometryWKT).Select(g => g.Key).ToList();


                LoadPage(route, _currentPage);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error! Unable to retrieve information", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }

        }
        void LoadPage(RouteView route, int page)
        {
            var count = _streetNames.Count.ToString();
            Shell.Current.DisplayAlert("LoadPage", count, "Ok");
            var paginatedStreetNames = _streetNames.Skip((page - 1) * _pageSize).Take(_pageSize);

            route.Streets.Clear();
            foreach (var name in paginatedStreetNames)
            {
                route.Streets.Add(name);
            }
            var routeCount = route.Streets.Count.ToString();

            Shell.Current.DisplayAlert("Route", routeCount, "Ok");
        }

        [RelayCommand]
        void LoadNextPage(RouteView route)
        {
            if (_currentPage * _pageSize < _streetNames.Count)
            {
                _currentPage++;
                LoadPage(route, _currentPage);
            }
        }

        [RelayCommand]
        void LoadPreviousPage(RouteView route)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadPage(route, _currentPage);
            }
        }
        [RelayCommand]
        async Task ShowOnMap(long osmId)
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                //get osm data
                var serializedStreets = JsonSerializer.Serialize(_streetGeometries);

                await Shell.Current.GoToAsync($"{nameof(MapView)}?Streets={serializedStreets}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get routes: {ex.Message}");
                await Shell.Current.DisplayAlert("Error!", "Unable to load Streets on Map", "OK");

            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task DownloadOffline(long osmId)
        {
            if(IsBusy)
                return;
            try
            {
                var response = await Shell.Current.DisplayAlert("Download", "Route Data will be downloaded to your device", "Proceed", "Cancel");
                if (!response)
                    return;
                IsBusy = true;
                var routeView = Routes.Where(route => route.Osm_Id == osmId).FirstOrDefault();
                routeView.IsDownloaded = true;

                var route = await _routeService.GetRouteByOsmIdAsync(osmId);
                if (route is not null)
                {
                    route.StreetNameSaved = true;
                    await _routeService.UpdateRouteAsync(route);
                }
                else
                {
                    Route newRoute = new Route
                    {
                        Osm_Id = routeView.Osm_Id,
                        Code = routeView.Code,
                        Name = routeView.Name,
                        StreetNameSaved = true,
                    };
                    await _routeService.InsertRouteAsync(newRoute);
                }

                foreach (var street in Streets)
                {
                    var data = await _streetService.GetStreetByIdAsync(street.StreetId);
                    if (data is not null)
                    {
                        data.RouteId = osmId;
                        data.GeometryWKT = street.GeometryWKT;
                        await _streetService.UpdateStreetAsync(data);
                    }
                    else
                        await _streetService.InsertStreetAsync(street);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error! Unable to download data, Please try again later", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task DeleteOfflineData(long osmId)
        {
            if (IsBusy)
                return;
            try
            {
                var response = await Shell.Current.DisplayAlert("Delete", "Are you sure you want to delete this Route Data?", "Yes", "Cancel");
                if (!response)
                    return;
                IsBusy = true;
                var routeView = Routes.Where(route => route.Osm_Id == osmId).FirstOrDefault();
                routeView.IsDownloaded = false;

                var route = await _routeService.GetRouteByOsmIdAsync(osmId);
                if (route is not null)
                {
                    route.StreetNameSaved = false;
                    await _routeService.UpdateRouteAsync(route);
                }

                foreach (var street in Streets)
                {
                    await _streetService.InsertStreetAsync(street);
                }
            }
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error! Failed delete data, Please try again later", ex.Message, "OK");
            }
            finally
            { 
                IsBusy = false; 
            }
        }

        [RelayCommand]
        async Task OpenOSM(long osmId)
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
                var uri = new Uri($"https://openstreetmap.org/relation/{osmId}");
                await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error! Unable to open OpenStreetMap", ex.Message, "OK");
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task Search()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;


                var pattern = @"^\d{2}[A-Za-z]$";
                var pattern1 = @"^\d{2}$";
                var pattern2 = @"^\d{1}$";
                var check = Regex.IsMatch(SearchInput, pattern);
                if (!check)
                    check = Regex.IsMatch(SearchInput, pattern1);
                if (!check)
                    check = Regex.IsMatch(SearchInput, pattern2);

                var results = new List<RouteView>();
                if (check)
                    results = Routes.Where(r => r.Code.Contains(SearchInput)).ToList();
                else
                {
                    if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        await Shell.Current.DisplayAlert("No connectivity!",
                            $"Offline mode can only search for Route Codes/Names", "OK");
                        return;
                    }
                    var codes = await _commuteMateApiService.SearchRoute(SearchInput);
                    results = _allRoutes.Where(route => codes.Contains(route.Code)).ToList();
                }

                if (SearchResults.Count != 0)
                    SearchResults.Clear();


                if (results != null)
                    foreach (var result in results)
                    {
                        var parts = result.Name.Split(new[] { ':' }, 2);
                        var name = parts.Length > 1 ? parts[1] : result.Name;
                        SearchResults.Add(new RouteView
                        {
                            Code = result.Code,
                            Osm_Id = result.Osm_Id,
                            Name = name,
                            IsStreetFrameVisible = false
                        });
                    }

                if (RoutesSearchBar.IsSoftInputShowing())
                    await RoutesSearchBar.HideSoftInputAsync(System.Threading.CancellationToken.None);

                Routes = new ObservableCollection<RouteView>(SearchResults);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to search location: {ex.Message}");
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        void CancelSearch()
        {
            if (SearchResults.Count != 0)
            {
                SearchResults.Clear();
                Routes = new ObservableCollection<RouteView>(_allRoutes);
                RoutesSearchBar.Text = string.Empty;
            }
            else
            {
                if (RoutesSearchBar.IsFocused)
                    RoutesSearchBar.Unfocus();

                CancelSearchButton.IsVisible = false;
            }
        }
    }
}
