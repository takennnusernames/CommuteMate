using PUV_Route_Recommender.Repositories;
using Vehicle = PUV_Route_Recommender.Models.Vehicle;

namespace PUV_Route_Recommender.Views;

public partial class RoutesPage : ContentPage
{
	public RoutesPage()
	{
		InitializeComponent();

        List<Vehicle> vehicles = VehicleRepository.GetVehicles();

        route_list.ItemsSource = vehicles;
    }
}