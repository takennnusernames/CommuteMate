using CommuteMate.Repositories;
using Vehicle = CommuteMate.Models.Vehicle;

namespace CommuteMate.Views;

public partial class InfoPage : ContentPage
{
	public InfoPage()
	{
		InitializeComponent();

		List<Vehicle> vehicles = VehicleRepository.GetVehicles();

		vehicle_list.ItemsSource = vehicles;

	}

    private async void vehicle_list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
		if(e.SelectedItem != null) 
		{
            await Shell.Current.GoToAsync($"{nameof(VehicleInfoPage)}?Id={((Vehicle)e.SelectedItem).Vehicle_ID}");
        }
    }

    private void vehicle_list_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        vehicle_list.SelectedItem = null;
    }
}