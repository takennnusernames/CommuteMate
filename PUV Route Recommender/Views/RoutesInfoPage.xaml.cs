using PUV_Route_Recommender.Models;
using PUV_Route_Recommender.Repositories;

namespace PUV_Route_Recommender.Views;

public partial class RoutesInfoPage : ContentPage
{
    public RoutesInfoPage(RouteInfoViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    
}