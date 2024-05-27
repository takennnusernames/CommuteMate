namespace CommuteMate.Views;

public partial class DetailsView : ContentView
{
    public DetailsView()
	{
		InitializeComponent();
	}

    public static readonly BindableProperty RouteProperty =
            BindableProperty.Create(nameof(Route), typeof(Route), typeof(DetailsView), default(Route));

    public Route Route
    {   
        get => (Route)GetValue(RouteProperty);
        set => SetValue(RouteProperty, value);
    }

    private ObservableCollection<string> streets;
    public ObservableCollection<string> Streets
    {
        get => streets;
        set
        {
            if (streets != value)
            {
                streets = value;
                OnPropertyChanged();
            }
        }
    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (StreetFrame.IsVisible)
            StreetFrame.IsVisible = false;
        else
        {
            StreetFrame.IsVisible = true;
            if (streets == null)
            {
                var frame = (Frame)sender;
                var route = (Route)frame.BindingContext;

                // Find the parent CollectionView
                var collectionView = GetParentCollectionView(frame);
                if (collectionView != null)
                {
                    var viewModel = collectionView.BindingContext as RoutesViewModel;
                    if (viewModel != null)
                    {
                        var streetNames = await viewModel.GoToDetails(route);
                        streets = new ObservableCollection<string>(streetNames);
                        StreetFrame.BindingContext = streets;
                        var check = StreetFrame.BindingContext;
                    }
                }
            }
        }
    }
    private CollectionView GetParentCollectionView(Element element)
    {
        Element parent = element.Parent;
        while (parent != null && !(parent is CollectionView))
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
}