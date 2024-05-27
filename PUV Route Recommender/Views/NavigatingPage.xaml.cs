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
using Microsoft.Maui;
using The49.Maui.BottomSheet;

namespace CommuteMate.Views;

public partial class NavigatingPage : ContentPage
{
    private readonly IMapServices _mapServices;
	public NavigatingPage(NavigatingViewModel viewModel, IMapServices mapServices)
	{
		InitializeComponent();
        _mapServices = mapServices;
        BindingContext = viewModel;
        //viewModel.mapControl = mapControl;
        viewModel.map = map;
        viewModel.CreateMapCommand.ExecuteAsync(map);
        viewModel.originSearchBar = originSearchBar;
        viewModel.destinationSearchBar = destinationSearchBar;
        viewModel.originCancel = originCancel;
        viewModel.destinationCancel = destinationCancel;
        viewModel.showDetailsButton = ShowDetailsButton;
        viewModel.GetRoutesButton = GetRoutesButton;
        viewModel.GetLocationButton = GetLocationButton;

    }
    private void BottomSheet_Dismissed(object sender, DismissOrigin e)
    {
        var viewModel = BindingContext as NavigatingViewModel;
        viewModel.ShowSlideUpButton(e);
    }

    private void map_MapClicked(object sender, Microsoft.Maui.Controls.Maps.MapClickedEventArgs e)
    {
        var viewModel = BindingContext as NavigatingViewModel;
        Task.FromResult(viewModel.MapClicked(e));
    }

    private void Pin_MarkerClicked(object sender, Microsoft.Maui.Controls.Maps.PinClickedEventArgs e)
    {
        var viewModel = BindingContext as NavigatingViewModel;
        Task.FromResult(viewModel.PinMarkerClicked(sender,e));
    }

    private void originSearchBar_Focused(object sender, FocusEventArgs e)
    {
        originCancel.IsVisible = true;
        var viewModel = BindingContext as NavigatingViewModel;
        viewModel.Source = "Origin";
        GetLocationButton.IsVisible = true;
    }

    private void originSearchBar_Unfocused(object sender, FocusEventArgs e)
    {
        if(originSearchBar.Text is null || originSearchBar.Text == "")
            originCancel.IsVisible = false;

        GetLocationButton.IsVisible = false;
    }

    private void destinationSearchBar_Focused(object sender, FocusEventArgs e)
    {
        destinationCancel.IsVisible = true;
        var viewModel = BindingContext as NavigatingViewModel;
        viewModel.Source = "Destination";
        GetLocationButton.IsVisible = true;
    }

    private void destinationSearchBar_Unfocused(object sender, FocusEventArgs e)
    {
        if(destinationSearchBar.Text is null || destinationSearchBar.Text == "")
            destinationCancel.IsVisible = false;

        GetLocationButton.IsVisible = false;
    }
}