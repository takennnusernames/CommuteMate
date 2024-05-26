using The49.Maui.BottomSheet;

namespace CommuteMate.Views.SlideUpSheets;

public partial class SlideUpCard : BottomSheet
{
	public SlideUpCard(NavigatingViewModel viewModel)
	{
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void PrioritySelected(object sender, EventArgs e)
    {
        await SlideToView(RouteSelection,PrioritySelection, true);
    }

    private async void RouteSelected(object sender, EventArgs e)
    {
        await SlideToView(RouteDetails, RouteSelection, true);
    }

    private async void BackToPrioritySelect(object sender, EventArgs e)
    {
        await SlideToView(PrioritySelection, RouteSelection, false);
    }

    private async void BackToRouteSelect(object sender, EventArgs e)
    {
        await SlideToView(RouteSelection, RouteDetails, false);
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
}