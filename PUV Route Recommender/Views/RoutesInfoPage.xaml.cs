using CommuteMate.Models;
using CommuteMate.Repositories;

namespace CommuteMate.Views;

public partial class RoutesInfoPage : ContentPage
{
    public RoutesInfoPage(RouteInfoViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
    
}