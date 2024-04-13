using CommuteMate.Repositories;
using Vehicle = CommuteMate.Models.Vehicle;

namespace CommuteMate.Views;

[QueryProperty(nameof(Vehicle_ID),"Id")]
public partial class VehicleInfoPage : ContentPage
{
	private Vehicle _vehicle;
	public VehicleInfoPage()
	{
		InitializeComponent();
	}

	public string Vehicle_ID
	{
		set
		{
            _vehicle = VehicleRepository.GetVehicleById(int.Parse(value));
			lblName.Text = _vehicle.Type;
		}
	}
}