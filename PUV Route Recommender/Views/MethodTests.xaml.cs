using CommuteMate.Interfaces;
using NetTopologySuite.Geometries;

namespace CommuteMate.Views;

public partial class MethodTests : ContentPage
{
	private readonly IMapServices _mapServices;
	public MethodTests(IMapServices mapService)
    {
        _mapServices = mapService;
        InitializeComponent();
	}

    private async void Button_Pressed(object sender, EventArgs e)
    {
		Coordinate origin = new Coordinate(123.888767, 10.298491);
		Coordinate destination = new Coordinate(123.906199, 10.293772);
        try
        {
            await _mapServices.GetDirectionsAsync(origin, destination);
        }
        catch(Exception ex) 
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok");
        }
    }
}