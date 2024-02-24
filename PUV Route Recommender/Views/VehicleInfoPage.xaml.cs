using PUV_Route_Recommender.Repositories;
using Vehicle = PUV_Route_Recommender.Models.Vehicle;

namespace PUV_Route_Recommender.Views;

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