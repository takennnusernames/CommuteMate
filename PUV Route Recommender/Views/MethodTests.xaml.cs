using CommuteMate.Interfaces;
using Mapsui.UI.Maui;
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
		Coordinate origin = new Coordinate(123.89539, 10.31035);
		Coordinate destination = new Coordinate(123.90006, 10.30421);
        try
        {
            var map = await _mapServices.CreateMapAsync();
            await _mapServices.GetDirectionsAsync(origin, destination, map);

            mapControlTest.Map = map;
        }
        catch(Exception ex) 
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "Ok");
        }
    }
}