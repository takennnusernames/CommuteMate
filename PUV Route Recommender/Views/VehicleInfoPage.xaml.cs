using CommuteMate.Repositories;
using Vehicle = CommuteMate.Models.Vehicle;

namespace CommuteMate.Views;

public partial class VehicleInfoPage : ContentPage
{
	private Vehicle _vehicle;
	public VehicleInfoPage()
	{
		InitializeComponent();
	}

}