using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommuteMate.Interfaces;
using CommuteMate.Views;
using System.Text.RegularExpressions;

namespace CommuteMate.ViewModels
{
    public partial class RoutesViewModel : BaseViewModel
    {
        readonly IConnectivity _connectivity;
        readonly IRouteService _routeService;
        readonly IStreetService _streetService;
        readonly ICommuteMateApiService _commuteMateApiService;
        readonly IMapServices _mapServices;
        readonly IRouteStreetService _routeStreetService;
        readonly IDownloadsRepository _downloadsRepository;
        public RoutesViewModel(
            IConnectivity connectivity,
            IRouteService routeService,
            IStreetService streetService,
            ICommuteMateApiService commuteMateApiService,
            IMapServices mapServices,
            IRouteStreetService routeStreetService,
            IDownloadsRepository downloadsRepository)
        {
            Title = "Route List";
            _connectivity = connectivity;
            _routeService = routeService;
            _streetService = streetService;
            _commuteMateApiService = commuteMateApiService;
            _mapServices = mapServices;
            _routeStreetService = routeStreetService;
            _downloadsRepository = downloadsRepository;
        }
        //properties
        private ObservableCollection<RouteView> _allRoutes = [];

        private ObservableCollection<RouteView> _filteredRoutes = [];
        private ObservableCollection<OfflinePath> _downloads = [];

        public ObservableCollection<RouteView> Routes
        {
            get => _filteredRoutes;
            set
            {
                _filteredRoutes = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<OfflinePath> Downloads
        {
            get => _downloads;
            set
            {
                _downloads = value;
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
        [ObservableProperty]
        public bool nextPage = false;
        [ObservableProperty]
        public bool previousPage = false;

        public ObservableCollection<RouteView> SearchResults { get; } = [];
        public ObservableCollection<Street> Streets { get; } = [];
        public ObservableCollection<Street> UniqueStreets { get; } = [];
        public List<Street> _streets { get; set; } = [];
        [ObservableProperty]
        string searchInput;
        public Button getRoutesButton { get; set; }
        public SearchBar RoutesSearchBar { get; set; }
        public Button CancelSearchButton { get; set; }

        private List<Street> _uniqueStreets = [];
        private List<string> _streetGeometries = [];
       

        public ObservableCollection<string> StreetList = new();
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

                    RoutesSearchBar.IsEnabled = false;
                    RoutesSearchBar.Placeholder = "Can't search while offline";
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
                        Osm_Id = route.OsmId,
                        Name = name,
                        IsStreetFrameVisible = false,
                        IsDownloaded = route.StreetNameSaved
                    };
                    Routes.Add(newRoute);
                    _allRoutes.Add(newRoute);
                }

                RoutesSearchBar.IsEnabled = true;
                RoutesSearchBar.Placeholder = "Search Street Name or Route Code/Name";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get routes: {ex.Message}");
                await Shell.Current.DisplayAlert("Alert!", "Press Refresh Routes button to Load Routes", "OK");

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

                if (SearchResults.Count != 0)
                    SearchResults.Clear();

                if (Downloads.Count > 0)
                {
                    Routes = new ObservableCollection<RouteView>(_allRoutes);
                    Downloads.Clear();
                    if(_allRoutes.Count > 0)
                        return;
                }
                var routes = await _commuteMateApiService.GetRoutes();


                Routes = new ObservableCollection<RouteView>(_allRoutes);

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
                        Osm_Id = route.OsmId,
                        Name = name,
                        IsStreetFrameVisible = false,
                        IsDownloaded = route.StreetNameSaved
                    };
                    Routes.Add(newRoute);
                    _allRoutes.Add(newRoute);
                }

                getRoutesButton.IsEnabled = false;


                RoutesSearchBar.IsEnabled = true;
                RoutesSearchBar.Placeholder = "Search Street Name or Route Code/Name";

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
        async Task GoToDetails(RouteView route)
        {

            if (IsBusy)
                return;
            try
            {
                IsBusy = true;

                Task.Delay(100).Wait();
                await Shell.Current.GoToAsync($"{nameof(RouteDetailsView)}", true,
                    new Dictionary<string, object>
                    {
                    {"Route", route},
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
                if(Routes.Any(route => route.IsStreetFrameVisible))
                {
                    foreach (var r in Routes.Where(route => route.IsStreetFrameVisible))
                    {
                        r.IsStreetFrameVisible = false;
                    }
                }
                IsBusy = true;
                if(route.IsDownloaded)
                    _streets = await _routeStreetService.GetRelatedStreets(route.Osm_Id);
                else
                {
                    if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        await Shell.Current.DisplayAlert("No connectivity!",
                            $"Please check internet and try again.", "OK");
                        return;
                    }

                    _streets = await _commuteMateApiService.GetRouteStreets(route.Osm_Id) ?? throw new Exception("streets is null");
                }

                route.IsStreetFrameVisible = true;
                //_streetGeometries.Clear();
                //_streetGeometries = _streets.GroupBy(s => s.GeometryWKT).Select(g => g.Key).ToList();

                Streets.Clear();
                foreach (var street in _streets)
                {
                    Streets.Add(street);
                }

                LoadPage(_currentPage);
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
        void LoadPage(int page)
        {
            _uniqueStreets.Clear();
            _uniqueStreets = _streets.GroupBy(s => s.Name).Select(g => g.First()).ToList();
            var paginatedStreetNames = _uniqueStreets.Skip((page - 1) * _pageSize).Take(_pageSize).ToList();

            //StreetList.Clear();
            //foreach (var name in paginatedStreetNames)
            //{
            //    StreetList.Add(name);
            //}

            UniqueStreets.Clear();
            foreach (var street in paginatedStreetNames)
            {
                UniqueStreets.Add(street);
            }
            if (_currentPage * _pageSize < _uniqueStreets.Count)
                NextPage = true;
        }

        [RelayCommand]
        void LoadNextPage()
        {
            if (_currentPage * _pageSize < _uniqueStreets.Count)
            {
                _currentPage++;
                LoadPage(_currentPage);
            }
            if (_currentPage * _pageSize < _uniqueStreets.Count)
                NextPage = true;
            else
                NextPage = false;

            if (_currentPage > 1)
                PreviousPage = true;
            else
                PreviousPage = false;
        }

        [RelayCommand]
        void LoadPreviousPage()
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadPage(_currentPage);
            }

            if (_currentPage * _pageSize < _uniqueStreets.Count)
                NextPage = true;
            else
                NextPage = false;

            if (_currentPage > 1)
                PreviousPage = true;
            else
                PreviousPage = false;
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
                var streetGeoms = Streets.Select(g => g.GeometryWKT).ToList();
                var serializedStreets = JsonSerializer.Serialize(streetGeoms);

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
        async Task Confirmation(RouteView route)
        {
            if(IsBusy)
                return;
            try
            {
                if (route.IsDownloaded)
                {
                    var response = await Shell.Current.DisplayAlert("Delete", "Are you sure you want to delete this Route Data?", "Yes", "Cancel");
                    if (!response)
                        return;
                    else
                        await DeleteOfflineData(route.Osm_Id);
                }
                else
                {
                    var response = await Shell.Current.DisplayAlert("Download", "Route Data will be downloaded to your device", "Proceed", "Cancel");
                    if (!response)
                        return;
                    else
                        await DownloadOffline(route.Osm_Id);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        async Task DownloadOffline(long osmId)
        {
            if(IsBusy)
                return;
            try
            {
                IsBusy = true;
                var routeView = Routes.Where(route => route.Osm_Id == osmId).FirstOrDefault();

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
                        OsmId = routeView.Osm_Id,
                        Code = routeView.Code,
                        Name = routeView.Name,
                        StreetNameSaved = true,
                    };
                    route = await _routeService.InsertRouteAsync(newRoute);
                }

                foreach (var street in Streets)
                {
                    var data = await _streetService.GetStreetByIdAsync(street.StreetId);
                    if (data is not null)
                    {
                        data.GeometryWKT = street.GeometryWKT;
                        try
                        {
                            await _streetService.UpdateStreetAsync(data);
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else
                    {
                        try
                        {
                            await _streetService.InsertStreetAsync(street);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    try
                    {
                        await _routeStreetService.CreateRelation(new RouteStreet
                        {
                            Street = street,
                            Route = route
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                routeView.IsDownloaded = true;
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

        async Task DeleteOfflineData(long osmId)
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                var routeView = Routes.Where(route => route.Osm_Id == osmId).FirstOrDefault();
                routeView.IsDownloaded = false;

                var route = await _routeService.GetRouteByOsmIdAsync(osmId);
                if (route is not null)
                {
                    route.StreetNameSaved = false;
                    await _routeService.UpdateRouteAsync(route);
                }

                //foreach (var street in Streets)
                //{
                //    await _streetService.InsertStreetAsync(street);
                //}
            }
            catch(Exception ex)
            {
                await Shell.Current.DisplayAlert("Error! Failed to delete data, Please try again later", ex.Message, "OK");
            }
            finally
            { 
                IsBusy = false; 
            }
        }
        
        async Task DeleteOfflinePath(OfflinePath offlinePath)
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                await _downloadsRepository.DeleteOfflinePathAsync(offlinePath);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error! Failed to delete data, Please try again later", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task ShowDownloads()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                List<Route> routes = [];

                var downloads = await _downloadsRepository.GetAllOfflinePathsAsync();
                if (downloads.Count != _downloads.Count)
                {
                    _downloads = new ObservableCollection<OfflinePath>(downloads);
                    Downloads = new ObservableCollection<OfflinePath>(_downloads);
                }
                routes = await _routeService.GetOfflineRoutesAsync();

                if (SearchResults.Count != 0)
                    SearchResults.Clear();


                if (routes != null)
                {
                    foreach (var result in routes)
                    {
                        var parts = result.Name.Split(new[] { ':' }, 2);
                        var name = parts.Length > 1 ? parts[1] : result.Name;
                        SearchResults.Add(new RouteView
                        {
                            Code = result.Code,
                            Osm_Id = result.OsmId,
                            Name = name,
                            IsStreetFrameVisible = false,
                            IsDownloaded = result.StreetNameSaved
                        });
                    }
                    getRoutesButton.IsEnabled = true;
                }

                if (SearchResults.Count() < 1 && Downloads.Count() < 1)
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                    string text = "You have no downloads";
                    ToastDuration duration = ToastDuration.Short;
                    double fontSize = 14;

                    var toast = Toast.Make(text, duration, fontSize);

                    await toast.Show(cancellationTokenSource.Token);
                    Routes = new ObservableCollection<RouteView>(_allRoutes) ;
                }
                else
                    Routes = new ObservableCollection<RouteView>(SearchResults);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get downloads: {ex.Message}");
                await Shell.Current.DisplayAlert("There was an error in retrieving downloads", "Please try restarting your application", "OK");

            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task ShowDownloadedPath(OfflinePath offlinePath)
        {
            if (IsBusy)
                return;
            try
            {
                //var steps = await _downloadsRepository.GetPathStep(offlinePath.Id);

                var response = await Shell.Current.DisplayAlert("", "Select Action", "Open on Map", "Delete");
                if (!response)
                {
                    await DeleteOfflinePath(offlinePath);
                    await ShowDownloads();
                    return;
                }

                IsBusy = true;
                var navigationParameter = new ShellNavigationQueryParameters
                {
                    { "Path", offlinePath }
                };

                await Shell.Current.GoToAsync($"{nameof(OfflinePathMapView)}", navigationParameter);
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

                if (SearchResults.Count() < 1)
                {
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                    string text = "You have no downloads";
                    ToastDuration duration = ToastDuration.Short;
                    double fontSize = 14;

                    var toast = Toast.Make(text, duration, fontSize);

                    await toast.Show(cancellationTokenSource.Token);
                }
                else
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
