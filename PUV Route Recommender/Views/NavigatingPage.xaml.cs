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
using CommuteMate.Interfaces;

namespace CommuteMate.Views;

public partial class NavigatingPage : ContentPage
{
    private readonly IMapServices _mapServices;
	public NavigatingPage(NavigatingViewModel viewModel, IMapServices mapServices)
	{
		InitializeComponent();
        _mapServices = mapServices;
        BindingContext = viewModel;
        viewModel.mapControl = mapControl;
        viewModel.CreateMapCommand.ExecuteAsync(mapControl.Map);
    }
    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();
    //}

}