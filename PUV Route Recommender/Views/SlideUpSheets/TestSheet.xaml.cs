using The49.Maui.BottomSheet;

namespace CommuteMate.Views.SlideUpSheets;

public partial class TestSheet : BottomSheet
{
	public TestSheet()
	{
		InitializeComponent();
	}
    void Resize()
    {
        divider.HeightRequest = 32;
    }

    public VisualElement Divider => divider;
}