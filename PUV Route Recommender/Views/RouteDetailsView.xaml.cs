namespace CommuteMate.Views;

public partial class RouteDetailsView : ContentPage
{
	public RouteDetailsView(RouteDetailsViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
        IsBusy = true;
    }
    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        IsBusy = true;
        await Task.Delay(100); // Adjust the delay time as needed
        //var viewModel = BindingContext as RouteDetailsViewModel;
        //await viewModel.GetStreetsCommand.ExecuteAsync(viewModel);
        IsBusy = false;
    }
}