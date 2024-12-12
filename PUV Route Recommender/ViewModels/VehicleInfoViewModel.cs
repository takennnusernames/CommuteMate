using CommuteMate.Interfaces;

namespace CommuteMate.ViewModels
{
    public partial class VehicleInfoViewModel : BaseViewModel
    {
        readonly IVehicleRepository _vehicleRepository;
        readonly ICommuteMateApiService _commuteMateApiService;
        public VehicleInfoViewModel(IVehicleRepository vehicleRepository, ICommuteMateApiService commuteMateApiService)
        {
            _vehicleRepository = vehicleRepository;
            _commuteMateApiService = commuteMateApiService;
        }
        public ObservableCollection<Vehicle> Vehicles { get; } = [];

        public Task GetVehicles()
        {   
            if(Vehicles.Count == 0)
            {
                List<Vehicle> vehicles = _commuteMateApiService.GetVehicles().Result;

                foreach (var vehicle in vehicles)
                {
                    Vehicles.Add(vehicle);
                }
            }
            return Task.FromResult(Vehicles);
        }

        [RelayCommand]
        async Task GetVehiclesAsync()
        {
            if (IsBusy)
                return;
            try
            {
                IsBusy = true;
                if (Vehicles.Count == 0)
                {
                    List<Vehicle> vehicles = await _commuteMateApiService.GetVehicles();

                    foreach (var vehicle in vehicles)
                    {
                        Vehicles.Add(vehicle);
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Unable to get vehicles: {ex.Message}");
                await Shell.Current.DisplayAlert("Error in retrieveing Vehicle List", "Please check your internet connection", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
