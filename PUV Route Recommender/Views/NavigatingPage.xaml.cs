using The49.Maui.BottomSheet;

namespace CommuteMate.Views;
[QueryProperty(nameof(OfflinePath), "Path")]
public partial class NavigatingPage : ContentPage
{
    OfflinePath path;
    public OfflinePath Path
    {
        get => path;
        set
        {
            path = value;
            OnPropertyChanged();
        }
    }
    
    public NavigatingPage(NavigatingViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        viewModel.Map = map;
        viewModel.CreateMapCommand.ExecuteAsync(map);
        viewModel.OriginSearchBar = originSearchBar;
        viewModel.DestinationSearchBar = destinationSearchBar;
        viewModel.OriginCancel = originCancel;
        viewModel.DestinationCancel = destinationCancel;
        viewModel.ShowDetailsButton = ShowDetailsButton;
        viewModel.GetRoutesButton = GetRoutesButton;
        viewModel.GetLocationButton = GetLocationButton;
        if (path != null)
            viewModel.deserializeOfflinePathCommand.ExecuteAsync(path);
    }
    private void BottomSheet_Dismissed(object sender, DismissOrigin e)
    {
        var viewModel = BindingContext as NavigatingViewModel;
        viewModel.ShowSlideUpButton();
    }

    private void Map_MapClicked(object sender, Microsoft.Maui.Controls.Maps.MapClickedEventArgs e)
    {
        var viewModel = BindingContext as NavigatingViewModel;
        Task.FromResult(viewModel.MapClicked(e));
    }

    private void Pin_MarkerClicked(object sender, Microsoft.Maui.Controls.Maps.PinClickedEventArgs e)
    {
        var viewModel = BindingContext as NavigatingViewModel;
        Task.FromResult(viewModel.PinMarkerClicked(sender,e));
    }

    private void OriginSearchBar_Focused(object sender, FocusEventArgs e)
    {
        originCancel.IsVisible = true;
        var viewModel = BindingContext as NavigatingViewModel;
        viewModel.Source = "Origin";
        GetLocationButton.IsVisible = true;
    }

    private void OriginSearchBar_Unfocused(object sender, FocusEventArgs e)
    {
        if(originSearchBar.Text is null || originSearchBar.Text == "")
            originCancel.IsVisible = false;

        GetLocationButton.IsVisible = false;
    }

    private void DestinationSearchBar_Focused(object sender, FocusEventArgs e)
    {
        destinationCancel.IsVisible = true;
        var viewModel = BindingContext as NavigatingViewModel;
        viewModel.Source = "Destination";
        GetLocationButton.IsVisible = true;
    }

    private void DestinationSearchBar_Unfocused(object sender, FocusEventArgs e)
    {
        if(destinationSearchBar.Text is null || destinationSearchBar.Text == "")
            destinationCancel.IsVisible = false;

        GetLocationButton.IsVisible = false;
    }

}