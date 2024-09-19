using CommuteMate.Repositories;
using Vehicle = CommuteMate.Models.Vehicle;

namespace CommuteMate.Views;

public partial class VehicleInfoPage : ContentPage
{
	public VehicleInfoPage(VehicleInfoViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
	}

    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    var viewModel = BindingContext as VehicleInfoViewModel;
    //    viewModel.GetVehicles();
    //}

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        IsBusy = true;
        await Task.Delay(100); // Adjust the delay time as needed
        var viewModel = BindingContext as VehicleInfoViewModel;
        await viewModel.GetVehiclesCommand.ExecuteAsync(viewModel);
        IsBusy = false;
    }

}