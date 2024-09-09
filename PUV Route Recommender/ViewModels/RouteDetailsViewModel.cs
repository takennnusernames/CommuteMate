using CommuteMate.Interfaces;
using CommuteMate.Views;

namespace CommuteMate.ViewModels
{
    [QueryProperty(nameof(Route), "Route")]
    public partial class RouteDetailsViewModel(
        IStreetService streetService,
        IConnectivity connectivity,
        ICommuteMateApiService commuteMateApiService,
        IRouteService routeService) : BaseViewModel
    {
        readonly IStreetService _streetService = streetService;
        readonly IConnectivity _connectivity = connectivity;
        readonly ICommuteMateApiService _commuteMateApiService = commuteMateApiService;
        readonly IRouteService _routeService = routeService;

        public ObservableCollection<Street> Streets { get; set; } = [];

        private List<string> _streetNames = [];
        //public ObservableCollection<string> StreetNames
        //{
        //    get => _streetNames;
        //    set
        //    {
        //        _streetNames = value;
        //        OnPropertyChanged();
        //    }
        //}

        [ObservableProperty]
        RouteView route;

        private List<string> _streetGeometries = [];

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

        [RelayCommand]
        async Task GetStreets()
        {
            if (IsBusy)
                return;
            try
            {

                IsBusy = true;
                var streets = await _commuteMateApiService.GetRouteStreets(Route.Osm_Id) ?? throw new Exception("streets is null");
                //_streetNames = streets.GroupBy(s => s.Name).Select(g => g.Key).ToList();
                Streets.Clear();
                foreach (var street in streets)
                {
                    Streets.Add(street);
                }

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
        //void LoadPage(RouteView route, int page)
        //{
        //    //var count = _streetNames.Count.ToString();
        //    //Shell.Current.DisplayAlert("LoadPage", count, "Ok");
        //    var paginatedStreetNames = _streetNames.Skip((page - 1) * _pageSize).Take(_pageSize);

        //    StreetNames.Clear();
        //    foreach (var name in paginatedStreetNames)
        //    {
        //        StreetNames.Add(name);
        //    }
        //    //var routeCount = route.Streets.Count.ToString();

        //    //Shell.Current.DisplayAlert("Route", routeCount, "Ok");
        //}

        //[RelayCommand]
        //void LoadNextPage(RouteView route)
        //{
        //    if (_currentPage * _pageSize < _streetNames.Count)
        //    {
        //        _currentPage++;
        //        LoadPage(route, _currentPage);
        //    }
        //}

        //[RelayCommand]
        //void LoadPreviousPage(RouteView route)
        //{
        //    if (_currentPage > 1)
        //    {
        //        _currentPage--;
        //        LoadPage(route, _currentPage);
        //    }
        //}
        //[RelayCommand]
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
            if (IsBusy)
                return;
            try
            {
                var response = await Shell.Current.DisplayAlert("Download", "Route Data will be downloaded to your device", "Proceed", "Cancel");
                if (!response)
                    return;
                IsBusy = true;
                //var routeView = Routes.Where(route => route.Osm_Id == osmId).FirstOrDefault();
                //routeView.IsDownloaded = true;

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
                        Osm_Id = route.Osm_Id,
                        Code = route.Code,
                        Name = route.Name,
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
                //var routeView = Routes.Where(route => route.Osm_Id == osmId).FirstOrDefault();
                //routeView.IsDownloaded = false;

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
            catch (Exception ex)
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
    }
}
