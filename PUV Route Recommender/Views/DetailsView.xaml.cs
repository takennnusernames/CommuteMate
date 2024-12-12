using CommuteMate.Utilities;

namespace CommuteMate.Views;

public partial class DetailsView : ContentView
{
    public DetailsView()
	{
		InitializeComponent();
        //OpenMapButton.BindingContext = streets;
    }

    public static readonly BindableProperty RouteProperty =
            BindableProperty.Create(nameof(Route), typeof(Route), typeof(DetailsView), default(Route));

    public Route Route
    {   
        get => (Route)GetValue(RouteProperty);
        set => SetValue(RouteProperty, value);
    }

    private ObservableCollection<string> streetNames;
    public ObservableCollection<string> StreetNames
    {
        get => streetNames;
        set
        {
            if (streetNames != value)
            {
                streetNames = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<List<string>> StreetList = new();


    //private async void RouteTapped(object sender, TappedEventArgs e)
    //{
    //    try
    //    {
    //        if (StreetFrame.IsVisible)
    //        {
    //            StreetFrame.IsVisible = false;
    //            var frame = (Frame)sender;

    //            // Find the parent CollectionView
    //            var collectionView = GetParentCollectionView(frame);
    //            if (collectionView != null)
    //            {
    //                collectionView.IsEnabled = true;
    //                //collectionView.Behaviors.Clear();
    //            }
    //        }
    //        else
    //        {
    //            StreetFrame.IsVisible = true;

    //            var frame = (Frame)sender;
    //            var route = (Route)frame.BindingContext;
    //            // Find the parent CollectionView
    //            var collectionView = GetParentCollectionView(frame);
    //            if (collectionView != null)
    //            {
    //                ScrollToTop(collectionView, frame);
    //                if (streets == null)
    //                {
    //                    var viewModel = collectionView.BindingContext as RoutesViewModel;
    //                    if (viewModel != null)
    //                    {
    //                        streets = await viewModel.GetStreets(route);
    //                        var streetNames = streets.GroupBy(s => s.Name).Select(g => g.Key).ToList();
    //                        int pageCount = (int)Math.Ceiling((double)streetNames.Count / 6);
    //                        List<List<string>> streetList = [];
    //                        int streetNumber = 0;
    //                        while (pageCount > 0)
    //                        {
    //                            List<string> page = [];
    //                            int count = 0;
    //                            while (count < 6)
    //                            {
    //                                if (streetNumber >= streetNames.Count)
    //                                    break;
    //                                page.Add(streetNames[streetNumber]);
    //                                streetNumber++;
    //                                count++;
    //                            }
    //                            StreetList.Add(page);
    //                            pageCount--;
    //                        }

    //                        StreetCollection.BindingContext = StreetList;
    //                        OsmButton.BindingContext = route;
    //                    }
    //                }
    //                //collectionView.Behaviors.Add(new DisableParentScrollBehavior());
    //            }

    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"Error: {ex.Message}");

    //        await Shell.Current.DisplayAlert("Error! Unable to retrieve information", ex.Message, "OK");
    //    }
    //}
    private IEnumerable<string> Flatten(List<List<string>> nestedList)
    {
        return nestedList.SelectMany(innerList => innerList);
    }
    private void ScrollToTop(CollectionView collectionView, Frame frame)
    {
        var itemIndex = collectionView.ItemsSource.Cast<object>().ToList().IndexOf(frame.BindingContext);
        if (itemIndex >= 0)
        {
            collectionView.ScrollTo(itemIndex, position: ScrollToPosition.Start, animate: true);
        }
    }
    static CollectionView GetParentCollectionView(Element element)
    {
        Element parent = element.Parent;
        while (parent != null && parent is not CollectionView)
        {
            parent = parent.Parent;
        }
        return parent as CollectionView;
    }
    private async Task SlideToView(View newView, View oldView, bool isForward)
    {
        double width = this.Width;
        newView.TranslationX = isForward ? width : -width;
        newView.IsVisible = true;

        var slideOutTask = oldView.TranslateTo(isForward ? -width : width, 0, 250, Easing.SinInOut);
        var slideInTask = newView.TranslateTo(0, 0, 250, Easing.SinInOut);

        await Task.WhenAll(slideOutTask, slideInTask);

        oldView.IsVisible = false;
    }

    private void Open_Button_Clicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var route = (Route)button.BindingContext;

        // Find the parent CollectionView
        var collectionView = GetParentCollectionView(button);
        //if (collectionView != null)
        //{
        //    var viewModel = collectionView.BindingContext as RoutesViewModel;
        //    if (viewModel != null)
        //    {
        //        await viewModel.ShowOnMap(streets);
        //    }
        //}
    }

    private async void OSM_Button_Clicked_1(object sender, EventArgs e)
    {
        var uri = new Uri("https://openstreetmap.org"); // Replace with your desired URL
        await Browser.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }
}