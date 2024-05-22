using The49.Maui.BottomSheet;
namespace CommuteMate.Views.SlideUpSheets;

public partial class RoutePathDetails : BottomSheet
{
	public RoutePathDetails(NavigatingViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}