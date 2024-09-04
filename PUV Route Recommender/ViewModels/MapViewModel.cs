using CommuteMate.Interfaces;
using GoogleMap = Microsoft.Maui.Controls.Maps.Map;

namespace CommuteMate.ViewModels
{
    public partial class MapViewModel : BaseViewModel
    {
        readonly IMapServices _mapServices;
        readonly IConnectivity _connectivity;
        public MapViewModel(IMapServices mapServices, IConnectivity connectivity)
        {
            _connectivity = connectivity;   
            _mapServices = mapServices; 

        }
        public GoogleMap Map { get; set; }
        [RelayCommand]
        async Task CreateMap()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await Shell.Current.DisplayAlert("No connectivity!",
                        $"Please check internet and try again.", "OK");
                    return;
                }

                await _mapServices.CreateGoogleMapAsync(Map);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
