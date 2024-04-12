using Mapsui;
using Mapsui.Extensions;
using Mapsui.Projections;
using Mapsui.Tiling;
using Map = Mapsui.Map;
using Microsoft.Maui.ApplicationModel;
using System.Net.Http;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Color = Mapsui.Styles.Color;
using Mapsui.Nts.Extensions;
using PUV_Route_Recommender.Interfaces;

namespace PUV_Route_Recommender.Views;

public partial class NavigatingPage : ContentPage
{
    private readonly IMapServices _mapServices;
	public NavigatingPage(NavigatingViewModel viewModel, IMapServices mapServices)
	{
		InitializeComponent();
        _mapServices = mapServices;
        BindingContext = viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        mapControl.Map = await _mapServices.CreateMapAsync();
    }
    
}