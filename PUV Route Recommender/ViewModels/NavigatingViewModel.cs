using Mapsui.Layers;
using CommuteMate.Interfaces;
using Mapsui.UI.Maui;
using CommuteMate.Views.SlideUpSheets;

namespace CommuteMate.ViewModels
{
    public partial class NavigatingViewModel : BaseViewModel
    {
        //dependecy injections
        readonly IMapServices _mapServices;
        readonly IConnectivity _connectivity;
        readonly ICommuteMateApiService _commuteMateApiService;
        readonly IStreetService _streetService;
        readonly RoutePathSelection selection;
        readonly RoutePathDetails details;
        public NavigatingViewModel(IStreetService streetService, IMapServices mapServices, ICommuteMateApiService commuteMateApiService, IConnectivity connectivity)
        {
            Title = "Map";
            _streetService = streetService;
            _mapServices = mapServices;
            _connectivity = connectivity;
            _commuteMateApiService = commuteMateApiService;


            selection = new RoutePathSelection(this);
            details = new RoutePathDetails(this);
        }

        //properties
        [ObservableProperty]
        string originText;
        [ObservableProperty]
        LocationDetails originLocation;


        [ObservableProperty]
        string destinationText;
        [ObservableProperty]
        LocationDetails destinationLocation;

        [ObservableProperty]
        string source;

        [ObservableProperty]
        ObservableCollection<LocationDetails> searchResults = new();

        [ObservableProperty]
        ObservableCollection<RoutePath> pathOptions = new();

        [ObservableProperty]
        RoutePath currentPath = new();

        public MapControl mapControl { get; set; }
        public SearchBar originSearchBar { get; set; }
        public SearchBar destinationSearchBar { get; set; }



        [RelayCommand]
        async Task CreateMap()
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

                mapControl.Map = await _mapServices.CreateMapAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task GetLocationAsync(string source)
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
                try
                {
                    var location = await _mapServices.GetCurrentLocationAsync();
                    if(location == null)
                        await Shell.Current.DisplayAlert("Out of Bounds", $"Make sure to select a location within Cebu City", "OK");
                    else
                    {
                        Source = source;
                        if (Source == "Origin")
                        {
                            OriginText = location.Name;
                            OriginLocation = location;
                        }
                        else if (Source == "Destination")
                        {
                            DestinationText = location.Name;
                            DestinationLocation = location;
                        }

                        SearchResults.Clear();
                    }


                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error", $"An error occurred while searching for the location: {ex.Message}", "OK");
                    return;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to get location: {ex.Message}");
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task SearchLocation(string source)
        {
            if (IsBusy)
                return;
            try
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("No connectivity!",
                        $"Please check internet and try again.", "OK");
                    return;
                }

                IsBusy = true;
                string text;
                if (source == "Origin")
                {
                    text = OriginText;
                }
                else if (source == "Destination")
                    text = DestinationText;
                else
                    text = "";

                var locations = await _mapServices.GoogleSearchLocationAsync(text);

                if (SearchResults.Count != 0)
                    SearchResults.Clear();

                foreach (var location in locations)
                    SearchResults.Add(location);

                if (originSearchBar.IsSoftInputShowing())
                    await originSearchBar.HideSoftInputAsync(System.Threading.CancellationToken.None);
                Source = source;
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
        void CancelSearch(string source)
        {
            if(source == "Origin")
            {
                OriginText = string.Empty;
                OnPropertyChanged(nameof(OriginText));
            }
            else if(source == "Destination")
            {
                DestinationText = string.Empty;
                OnPropertyChanged(nameof(DestinationText));
            }
            SearchResults.Clear();
            PathOptions.Clear();
        }

        [RelayCommand]
        async Task SelectLocation(LocationDetails location)
        {
            if(Source == "Origin")
            {
                OriginText = location.Name;
                OriginLocation = location;
            }
            else if(Source == "Destination")
            {
                DestinationText = location.Name;
                DestinationLocation = location;
            }
            await _mapServices.AddPin(mapControl.Map, location);

            SearchResults.Clear();
        }

        [RelayCommand]
        async Task GetRoutes()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;

                if (OriginLocation == null)
                    throw new Exception("Please Select Origin");
                if (DestinationLocation == null)
                    throw new Exception("Please Select Destination");

                try
                {
                    var layers = mapControl.Map.Layers.OfType<MemoryLayer>().ToList();
                    foreach(var layer in layers)
                    {
                        mapControl.Map.Layers.Remove(layer);
                    }

                    var origin = OriginLocation.Coordinate;
                    var destination = DestinationLocation.Coordinate;

                    var options = await _commuteMateApiService.GetPath(origin, destination);
                    //(Route, Path, Fare, Distance)
                    if (PathOptions.Count != 0)
                        PathOptions.Clear();

                    foreach (var option in options)
                        PathOptions.Add(option);

                    await selection.ShowAsync();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Getting Routes:", ex.Message);
                await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok");
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        async Task SelectPath(RoutePath path)
        {
            CurrentPath = path;
            foreach(var step in path.Steps)
            {
                if (step.Action.Contains("Walk"))
                    await _mapServices.addGeometry(mapControl.Map, step.StepGeometry, "dotted");
                else if (step.Action.Contains("Ride"))
                    await _mapServices.addGeometry(mapControl.Map, step.StepGeometry, "straight");
            }
            await selection.DismissAsync();
            await details.ShowAsync();
            mapControl.Map = mapControl.Map;
        }

        
    }
}
