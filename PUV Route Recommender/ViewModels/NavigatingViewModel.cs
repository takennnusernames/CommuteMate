using Mapsui.Layers;
using CommuteMate.Interfaces;
using CommuteMate.Views.SlideUpSheets;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
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
        public NavigatingViewModel(IMapServices mapServices, ICommuteMateApiService commuteMateApiService, IConnectivity connectivity)
        {
            Title = "Map";
            _mapServices = mapServices;
            _connectivity = connectivity;
            _commuteMateApiService = commuteMateApiService;
            SlideUpCard = new SlideUpCard(this);

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
        ObservableCollection<LocationDetails> searchResults = [];

        [ObservableProperty]
        ObservableCollection<RoutePath> pathOptions = [];

        [ObservableProperty]
        RoutePath currentPath = new();

        [ObservableProperty]
        ObservableCollection<RouteStep> currentPathSteps = [];

        [ObservableProperty]
        ObservableCollection<Pin> positions = [];

        [ObservableProperty]
        ObservableCollection<CustomPin> intersections = [];

        [ObservableProperty]
        ObservableCollection<Microsoft.Maui.Controls.Maps.Polyline> lines = [];
        public GoogleMap Map { get; set; }
        public SearchBar OriginSearchBar { get; set; }
        public Button OriginCancel { get; set; }
        public SearchBar DestinationSearchBar { get; set; }
        public Button DestinationCancel { get; set; }
        public Frame ShowDetailsButton { get; set; }
        public Button GetLocationButton { get; set; }
        public Button GetRoutesButton { get; set; }
        public SlideUpCard SlideUpCard {  get; set; }
        public CollectionView RouteStepsCollectionView { get; set; }

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
                var result = await Shell.Current.DisplayActionSheet("Make Location As:", "Cancel", null, ["Origin", "Destination"]);
                switch (result)
                {
                    case "Origin"://remove
                        if (OriginLocation != null)
                        {
                            var origin = new Microsoft.Maui.Devices.Sensors.Location(OriginLocation.Coordinate.Y, OriginLocation.Coordinate.X);
                            var originPin = Map.Pins.Where(p => p.Location == origin).FirstOrDefault();
                            await _mapServices.RemoveGooglePin(originPin, Map);
                            Positions.Remove(originPin);
                        }
                        OriginLocation = new LocationDetails
                        {
                            Coordinate = new NetTopologySuite.Geometries.Coordinate(e.Location.Longitude, e.Location.Latitude),
                            Name = "Origin"
                        };
                        Source = "Origin";
                        await SelectLocation(OriginLocation);
                        OriginText = e.Location.Longitude.ToString("F4") + "," + e.Location.Latitude.ToString("F4");
                        break;

                    case "Destination":
                        if(DestinationLocation != null)
                        {
                            var destination = new Microsoft.Maui.Devices.Sensors.Location(DestinationLocation.Coordinate.Y, DestinationLocation.Coordinate.X);
                            var destinationPin = Map.Pins.Where(p => p.Location == destination).FirstOrDefault();
                            await _mapServices.RemoveGooglePin(destinationPin, Map);
                            Positions.Remove(destinationPin);
                        }
                        DestinationLocation = new LocationDetails
                        {
                            Coordinate = new NetTopologySuite.Geometries.Coordinate(e.Location.Longitude, e.Location.Latitude),
                            Name = "Destination"
                        };
                        Source = "Destination";
                        await SelectLocation(DestinationLocation);
                        DestinationText = e.Location.Longitude.ToString("F4") + "," + e.Location.Latitude.ToString("F4");
                        break;

                    case "Cancel":
                        break;

                    default:
                        break;
                }
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
                    Map.Pins.Remove(pinInfo);
                    Positions.Remove(pinInfo);
                    break;
                default:
                    break;
            }
        }

        public async void ShowSlideUpButton()
        {
            ShowDetailsButton.IsVisible = true;
            await ShowDetailsButton.FadeTo(1, 500);
            ShowDetailsButton.Scale = 0.1;
            await ShowDetailsButton.ScaleTo(1, 250, Easing.CubicIn);
        }

        private void SetCollectionViewHeight()
        {
            // Get the screen height
            var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

            // Calculate 40% of the screen height for CollectionView max height
            double collectionViewHeight = screenHeight * 0.4;

            // Set the calculated height to MaxHeightRequest of the CollectionView
            RouteStepsCollectionView.MaximumHeightRequest = collectionViewHeight;
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

                await _mapServices.CreateGoogleMapAsync(Map);
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
        async Task GetLocationAsync()
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
                        await SelectLocation(location);
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

                if(locations != null)
                    foreach (var location in locations)
                        SearchResults.Add(location);

                if (OriginSearchBar.IsSoftInputShowing())
                    await OriginSearchBar.HideSoftInputAsync(System.Threading.CancellationToken.None);

                if (DestinationSearchBar.IsSoftInputShowing())
                    await DestinationSearchBar.HideSoftInputAsync(System.Threading.CancellationToken.None);
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
                if (!OriginSearchBar.IsFocused)
                    OriginCancel.IsVisible = false;

                if(OriginLocation != null)
                {
                    var pin = Positions.Where(p => p.Location == new Microsoft.Maui.Devices.Sensors.Location(OriginLocation.Coordinate.Y, OriginLocation.Coordinate.X)).FirstOrDefault();
                    Map.Pins.Remove(pin);
                    OriginLocation = null;
                }

            }
            else if(source == "Destination")
            {
                DestinationText = string.Empty;
                OnPropertyChanged(nameof(DestinationText));
                if (!DestinationSearchBar.IsFocused)
                    DestinationCancel.IsVisible = false;
                if(DestinationLocation != null)
                {
                    var pin = Positions.Where(p => p.Location == new Microsoft.Maui.Devices.Sensors.Location(DestinationLocation.Coordinate.Y, DestinationLocation.Coordinate.X)).FirstOrDefault();
                    Map.Pins.Remove(pin);
                    DestinationLocation = null;
                }
            }
            GetRoutesButton.IsVisible = false;
            SearchResults.Clear();
            PathOptions.Clear();
        }

        [RelayCommand]
        async Task SelectLocation(LocationDetails location)
        {
            if (Source == "Origin")
            {
                OriginText = location.Name;
                OriginLocation = location;
                OriginCancel.IsVisible = true;
            }
            else if(Source == "Destination")
            {
                DestinationText = location.Name;
                DestinationLocation = location;
                DestinationCancel.IsVisible = true;
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Unknown Source, Please enter from searchbar", "OK");
            }

            var newPin = await _mapServices.AddGooglePin(location, Map) ?? null;


            if (OriginText is not null && OriginText != "")
                if(DestinationText is not null && DestinationText != "")
                {
                    GetLocationButton.IsVisible = false;
                    GetRoutesButton.IsVisible = true;
                }


            if (newPin is not null)
                if (!Positions.Contains(newPin))
                    Positions.Add(newPin);

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
                    var origin = OriginLocation.Coordinate;
                    var destination = DestinationLocation.Coordinate;

                    var options = await _commuteMateApiService.GetPath(origin, destination);
                    if (options == null)
                    {
                        await Shell.Current.DisplayAlert("No optimal route found", "For better results, select an area closer to a highway", "Ok");
                        return;
                    }
                    if (PathOptions.Count != 0)
                        PathOptions.Clear();

                    foreach (var option in options)
                        PathOptions.Add(option);

                    // Trigger UI update through data binding
                    OnPropertyChanged(nameof(PathOptions));

                    await SlideUpCard.Refresh();

                    await SlideUpCard.ShowAsync();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok");
                    return;
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
        public async Task SelectPath(RoutePath path)
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                if (path == CurrentPath)
                    return;
                if (Lines.Count > 0)
                {
                    foreach (var pin in Intersections)
                    {
                        Map.Pins.Remove(pin);
                    }
                    foreach (var line in Lines)
                    {
                        Map.MapElements.Remove(line);
                    }
                    Intersections.Clear();
                    Lines.Clear();
                }
                CurrentPath = path;
                if (CurrentPathSteps.Count > 0)
                    CurrentPathSteps.Clear();
                foreach (var step in CurrentPath.Steps)
                    CurrentPathSteps.Add(step);

                Queue<Color> colorQueue = new Queue<Color>();
                colorQueue.Enqueue(Colors.Orange);
                colorQueue.Enqueue(Colors.Blue);
                colorQueue.Enqueue(Colors.Red);
                colorQueue.Enqueue(Colors.Green);
                colorQueue.Enqueue(Colors.Yellow);
                colorQueue.Enqueue(Colors.Gray);


                SetCollectionViewHeight();

                foreach (var step in path.Steps)
                {
                    if (step.Action.Contains("Walk"))
                    {
                        Lines.Add(await _mapServices.AddGooglePolyline(step.StepGeometry, Map, step.Action, Colors.Black));
                    }
                    else if (step.Action.Contains("Ride"))
                    {
                        var color = colorQueue.Dequeue();
                        Lines.Add(await _mapServices.AddGooglePolyline(step.StepGeometry, Map, step.Action, color));
                    }
                    else
                    {
                        var pin = await _mapServices.AddCustomPin(step.StepGeometry, Map, step.Action, step.Instruction);
                        Intersections.Add(pin);
                    }
                }

                MapSpan mapSpan = new MapSpan(new Microsoft.Maui.Devices.Sensors.Location(OriginLocation.Coordinate.Y, OriginLocation.Coordinate.X), 0.03, 0.03);
                Map.MoveToRegion(mapSpan);
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
        async Task ShowCard()
        {
            await SlideUpCard.ShowAsync();
        }

        [RelayCommand]
        async Task GoToPin(string location)
        {
            var pin = Positions.Where(p => p.Label == location).FirstOrDefault();
            await SlideUpCard.DismissAsync();
            MapSpan mapSpan = new MapSpan(pin.Location, 0.01, 0.01);
            Map.MoveToRegion(mapSpan);
        }
        public Task PrioritySelect(string priority)
        {
            List<RoutePath> prioritize = [];
            switch (priority)
            {
                case "Fare":
                    prioritize = PathOptions.OrderBy(p => p.Summary.TotalFare).ToList();
                    break;
                case "Duration":
                    prioritize = PathOptions.OrderBy(p => p.Summary.TotalDuration).ToList();
                    break;
                case "Distance":
                    prioritize = PathOptions.OrderBy(p => p.Summary.TotalDistance).ToList();
                    break;
                case "Rides":
                    prioritize = PathOptions.OrderBy(p => p.Steps.Select(s => s.Action.Contains("Ride")).Count()).ToList();
                    break;
                default:
                    break;
            }
            PathOptions.Clear();
            foreach(var option in prioritize)
            {
                PathOptions.Add(option);
            }

            return Task.FromResult(PathOptions);
        }

        
    }
}
