using CommuteMate.Interfaces;
using CommuteMate.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.ViewModels
{
    public class VehicleInfoViewModel : BaseViewModel
    {
        readonly IVehicleRepository _vehicleRepository;
        public VehicleInfoViewModel(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }
        public ObservableCollection<Vehicle> Vehicles { get; } = [];

        public Task GetVehicles()
        {
            List<Vehicle> vehicles = _vehicleRepository.GetVehicles();

            foreach(var vehicle in vehicles)
            {
                Vehicles.Add(vehicle);
            }
            //vehicle_list.ItemsSource = vehicles;
            return Task.FromResult(Vehicles);
        }
    }
}
