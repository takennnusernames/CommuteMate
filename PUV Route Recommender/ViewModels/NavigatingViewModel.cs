using Mapsui.Layers;
using CommuteMate.Interfaces;
using Mapsui.UI.Maui;
using CommuteMate.Views.SlideUpSheets;
using Microsoft.Maui.Controls.Maps;
using GoogleMap = Microsoft.Maui.Controls.Maps.Map;
using MapClickedEventArgs = Microsoft.Maui.Controls.Maps.MapClickedEventArgs;
using PinClickedEventArgs = Microsoft.Maui.Controls.Maps.PinClickedEventArgs;
using Pin = Microsoft.Maui.Controls.Maps.Pin;
using NetTopologySuite.Geometries;
using The49.Maui.BottomSheet;
using System.Linq;

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
        readonly SlideUpCard slideUpCard;
        public NavigatingViewModel(IStreetService streetService, IMapServices mapServices, ICommuteMateApiService commuteMateApiService, IConnectivity connectivity)
        {
            Title = "Map";
            _streetService = streetService;
            _mapServices = mapServices;
            _connectivity = connectivity;
            _commuteMateApiService = commuteMateApiService;
            selection = new RoutePathSelection(this);
            details = new RoutePathDetails(this);
            slideUpCard = new SlideUpCard(this);
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

        [ObservableProperty]
        ObservableCollection<Pin> positions = new();
        public MapControl mapControl { get; set; }
        public GoogleMap map { get; set; }
        public SearchBar originSearchBar { get; set; }
        public Button originCancel { get; set; }
        public SearchBar destinationSearchBar { get; set; }
        public Button destinationCancel { get; set; }
        public Button showDetailsButton { get; set; }
        public Button GetLocationButton { get; set; }
        public Button GetRoutesButton { get; set; }


        public async Task MapClicked(MapClickedEventArgs e)
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
                Pin newPin = null;
                var result = await Shell.Current.DisplayActionSheet("Make Location As:", "Cancel", null, ["Origin", "Destination"]);
                switch (result)
                {
                    case "Origin":
                        if (OriginLocation != null)
                        {
                            var origin = new Microsoft.Maui.Devices.Sensors.Location(OriginLocation.Coordinate.Y, OriginLocation.Coordinate.X);
                            var originPin = map.Pins.Where(p => p.Location == origin).FirstOrDefault();
                            await _mapServices.RemoveGooglePin(originPin, map);
                            Positions.Remove(originPin);
                        }
                        OriginText = e.Location.Longitude.ToString("F2") + "," + e.Location.Latitude.ToString("F2");
                        OriginLocation = new LocationDetails
                        {
                            Coordinate = new NetTopologySuite.Geometries.Coordinate(e.Location.Longitude, e.Location.Latitude),
                            Name = ""
                        };
                        Source = "Origin";
                        await SelectLocation(OriginLocation);
                        newPin = await _mapServices.AddGooglePin(e.Location, map, result);
                        break;

                    case "Destination":
                        if(DestinationLocation != null)
                        {
                            var destination = new Microsoft.Maui.Devices.Sensors.Location(DestinationLocation.Coordinate.Y, DestinationLocation.Coordinate.X);
                            var destinationPin = map.Pins.Where(p => p.Location == destination).FirstOrDefault();
                            await _mapServices.RemoveGooglePin(destinationPin, map);
                            Positions.Remove(destinationPin);
                        }
                        DestinationText = e.Location.Longitude.ToString("F2") + "," + e.Location.Latitude.ToString("F2");
                        DestinationLocation = new LocationDetails
                        {
                            Coordinate = new NetTopologySuite.Geometries.Coordinate(e.Location.Longitude, e.Location.Latitude),
                            Name = ""
                        };
                        Source = "Destination";
                        await SelectLocation(DestinationLocation);
                        newPin = await _mapServices.AddGooglePin(e.Location, map, result);
                        break;

                    case "Cancel":
                        break;

                    default:
                        // Handle unexpected result
                        break;
                }
                if(newPin is not null)
                    if (!Positions.Contains(newPin))
                        Positions.Add(newPin);
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
        public async Task PinMarkerClicked(object sender, PinClickedEventArgs e)
        {

            e.HideInfoWindow = false;
            var pinInfo = (Pin)sender;
            var pinCoordinate = new Coordinate(pinInfo.Location.Longitude, pinInfo.Location.Latitude);
            string action;
            if (pinInfo.Label == "Origin")
            {
                action = await Shell.Current.DisplayActionSheet(pinInfo.Address, "Cancel", null, ["Make this as Destination", "Remove"]);
            }
            else if (pinInfo.Label == "Destination")
            {
                action = await Shell.Current.DisplayActionSheet(pinInfo.Address, "Cancel", null, ["Make this as Origin", "Remove"]);
            }
            else
                action = null;
            switch (action)
            {
                case "Make this as Destination":
                    DestinationText = OriginText;
                    DestinationLocation = OriginLocation;

                    OriginText = "";
                    OriginLocation = null;
                    break;
                case "Make this as Origin":
                    OriginText = DestinationText;
                    OriginLocation = DestinationLocation;

                    DestinationText = "";
                    DestinationLocation = null;
                    break;
                case "Remove":
                    map.Pins.Remove(pinInfo);
                    Positions.Remove(pinInfo);
                    break;
                default:
                    break;
            }
        }

        public void ShowSlideUpButton(DismissOrigin e)
        {
            showDetailsButton.IsVisible = true;
        }


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

                await _mapServices.CreateGoogleMapAsync(map);
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
                if (!originSearchBar.IsFocused)
                    originCancel.IsVisible = false;

            }
            else if(source == "Destination")
            {
                DestinationText = string.Empty;
                OnPropertyChanged(nameof(DestinationText));
                if (!destinationSearchBar.IsFocused)
                    destinationCancel.IsVisible = false;
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
                originCancel.IsVisible = true;
            }
            else if(Source == "Destination")
            {
                DestinationText = location.Name;
                DestinationLocation = location;
                destinationCancel.IsVisible = true;
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Unknown Source, Please enter from searchbar", "OK");
            }
            await _mapServices.AddGooglePin(location, map);

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
                    //var layers = mapControl.Map.Layers.OfType<MemoryLayer>().ToList();
                    //foreach(var layer in layers)
                    //{
                    //    mapControl.Map.Layers.Remove(layer);
                    //}
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
                await _mapServices.AddGooglePolyline(step.StepGeometry, map, step.Action);
            }
            await selection.DismissAsync();
            await details.ShowAsync();
        }

        [RelayCommand]
        async Task ShowCard()
        {
            await slideUpCard.ShowAsync();
        }

        
    }
}
