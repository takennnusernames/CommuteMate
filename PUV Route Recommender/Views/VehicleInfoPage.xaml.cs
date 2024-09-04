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

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var viewModel = BindingContext as VehicleInfoViewModel;
        viewModel.GetVehicles();
    }

}