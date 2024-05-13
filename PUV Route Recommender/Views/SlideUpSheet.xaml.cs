using The49.Maui.BottomSheet;

namespace CommuteMate.Views;

public partial class SlideUpSheet : BottomSheet
{
	public SlideUpSheet(NavigatingViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}