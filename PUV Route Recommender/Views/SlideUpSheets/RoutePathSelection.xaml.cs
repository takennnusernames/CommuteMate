using The49.Maui.BottomSheet;

namespace CommuteMate.Views.SlideUpSheets;

public partial class RoutePathSelection : BottomSheet
{
	public RoutePathSelection(NavigatingViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}