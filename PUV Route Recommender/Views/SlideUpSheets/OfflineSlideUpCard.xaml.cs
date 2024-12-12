using CommuteMate.Interfaces;
using CommuteMate.Repositories;
using The49.Maui.BottomSheet;

namespace CommuteMate.Views.SlideUpSheets;

public partial class OfflineSlideUpCard : BottomSheet
{
    readonly IDownloadsRepository _downloadsRepository;
	public OfflineSlideUpCard(OfflinePath offlinePath)
	{
		InitializeComponent();
        BindingContext = offlinePath;
        SetCollectionViewHeight();
    }

    private void BottomSheet_Dismissed(object sender, DismissOrigin e)
    {
        var viewModel = BindingContext as NavigatingViewModel;
        if (viewModel != null)
        {
            viewModel.ShowSlideUpButton();
        }
    }

    private void SetCollectionViewHeight()
    {
        // Get the screen height
        var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;

        // Calculate 40% of the screen height for CollectionView max height
        double collectionViewHeight = screenHeight * 0.4;

        // Set the calculated height to MaxHeightRequest of the CollectionView
        RouteStepsCollectionView.MaximumHeightRequest = collectionViewHeight;
    }

}