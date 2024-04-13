using CommuteMate.Repositories;
using CommuteMate.Models;
using CommuteMate.Interfaces;

namespace CommuteMate.Views;
 
public partial class RoutesView : ContentPage
{
    public RoutesView(RoutesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}