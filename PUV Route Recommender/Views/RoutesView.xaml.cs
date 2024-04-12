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
}