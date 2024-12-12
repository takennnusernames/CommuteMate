using CommuteMate.Repositories;
using CommuteMate.Models;
using CommuteMate.Interfaces;
using Microsoft.Maui.Controls;

namespace CommuteMate.Views;
 
public partial class RoutesView : ContentPage
{
    public RoutesView(RoutesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
        viewModel.getRoutesButton = GetRoutesButton;
        viewModel.RoutesSearchBar = RoutesSearchBar;
        viewModel.CancelSearchButton = CancelSearchButton;

        IsBusy = true;
    }
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        IsBusy = true;
        GetRoutesButton.IsEnabled = false;
        await Task.Delay(100); // Adjust the delay time as needed
        var viewModel = BindingContext as RoutesViewModel;
        await viewModel.ShowRoutesCommand.ExecuteAsync(viewModel);
        IsBusy = false;
        GetRoutesButton.IsEnabled = true;
    }

    private void RoutesSearchBar_Focused(object sender, FocusEventArgs e)
    {
        CancelSearchButton.IsVisible = true;
    }

    private void RoutesSearchBar_Unfocused(object sender, FocusEventArgs e)
    {
        if(RoutesSearchBar.Text == string.Empty)
            CancelSearchButton.IsVisible = false;
    }

    private void RouteTapped(object sender, TappedEventArgs e)
    {

        var frame = (Frame)sender;
        var collectionView = GetParentCollectionView(frame);
        ScrollToTop(collectionView, frame);
        return;

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

    private void DownloadTapped(object sender, TappedEventArgs e)
    {
        var frame = (Frame)sender;
        var collectionView = GetParentCollectionView(frame);
        ScrollToTop(collectionView, frame);

        return;
    }
}