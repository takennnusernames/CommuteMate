using PUV_Route_Recommender.Repositories;
using PUV_Route_Recommender.Models;
using PUV_Route_Recommender.Interfaces;

namespace PUV_Route_Recommender.Views;
 
public partial class RoutesView : ContentPage
{
    public RoutesView(RoutesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();
    //    route_list.ItemsSource = await _overpassApiServices.GetOSMData();
    //}
    //private async void route_list_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    //{
    //    if (e.SelectedItem != null)
    //    {
    //        await Shell.Current.GoToAsync($"{nameof(RoutesInfoPage)}?Id={((Route)e.SelectedItem).Osm_Id}");
    //    }
    //}

    //private void route_list_ItemTapped(object sender, ItemTappedEventArgs e)
    //{
    //    route_list.SelectedItem = null;
    //}
}