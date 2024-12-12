using Microsoft.Maui.Controls;
using The49.Maui.BottomSheet;

namespace CommuteMate.Views.SlideUpSheets;

public partial class SlideUpCard : BottomSheet
{
    MediumDetent medium;
    RatioDetent full;
    public SlideUpCard(NavigatingViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
        full = new RatioDetent{
            Ratio = 0.75f
        };
        medium = new MediumDetent();
        viewModel.RouteStepsCollectionView = RouteStepsCollectionView;
        viewModel.DetailsGrid = DetailsGrid;
    }
    public async Task Refresh()
    {
        if (this.Detents.Contains(full))
        {
            this.Detents.Remove(full);
            await SlideToView(RouteSelection, RouteDetails, false);
        }

        if (this.Detents.Contains(medium))
        {
            this.Detents.Remove(medium);
            await SlideToView(PrioritySelection, RouteSelection, false);
        }
    }

    private async void PrioritySelected(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var viewModel = BindingContext as NavigatingViewModel;
        await viewModel.PrioritySelect(button.Text);
        this.Detents.Add(medium);
        await SlideToView(RouteSelection,PrioritySelection, true);
    }

    private async void RouteSelected(object sender, EventArgs e)
    {
        var frame = (Frame)sender;
        var route = (RoutePath)frame.BindingContext;

        // Find the parent CollectionView
        var collectionView = GetParentCollectionView(frame);
        if (collectionView != null)
        {
            var viewModel = collectionView.BindingContext as NavigatingViewModel;
            if (viewModel != null)
            {
                await viewModel.SelectPath(route);
            }
        }
        this.Detents.Add(full);
        await SlideToView(RouteDetails, RouteSelection, true);
    }

    private async void BackToPrioritySelect(object sender, EventArgs e)
    {
        await SlideToView(PrioritySelection, RouteSelection, false);
        this.Detents.Remove(medium);
    }

    private async void BackToRouteSelect(object sender, EventArgs e)
    {
        await SlideToView(RouteSelection, RouteDetails, false);
        this.Detents.Remove(full);
    }
    private async Task TransitionToView(View newView, View oldView)
    {
        // Fade out the old view
        await oldView.FadeTo(0, 250);
        oldView.IsVisible = false;

        // Set the new view as visible and fade it in
        newView.Opacity = 0;
        newView.IsVisible = true;
        await newView.FadeTo(1, 250);
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

    private CollectionView GetParentCollectionView(Element element)
    {
        Element parent = element.Parent;
        while (parent != null && !(parent is CollectionView))
        {
            parent = parent.Parent;
        }
        return parent as CollectionView;
    }

    private void BottomSheet_Dismissed(object sender, DismissOrigin e)
    {
        var viewModel = BindingContext as NavigatingViewModel;
        if (viewModel != null)
        {
            viewModel.ShowSlideUpButton();
        }
    }

}