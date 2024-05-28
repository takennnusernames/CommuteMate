using CommuteMate.Interfaces;
using CommuteMate.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Repositories
{
    public class VehicleRepository : IVehicleRepository
    {
        public List<Vehicle> _vehicles = new List<Vehicle>()
        {
            new Vehicle
            {
                VehicleID =0, 
                Type="Traditional Jeepney", 
                Info=new VehicleInfo
                {
                    MinimumFare = 12.0,
                    MinimumKM = 4,
                    Comfortability = 3,
                    FareRate = 1.80
                },
                ImageFileName="Sample/jeepney_sample2.jpg"
            },
            new Vehicle
            {
                VehicleID =1,
                Type="Modernized Jeepney",
                Info=new VehicleInfo
                {
                    MinimumFare = 14.0,
                    MinimumKM = 4,
                    Comfortability = 6,
                    FareRate = 1.80
                },
                ImageFileName="Sample/modern_jeepney_sample2.jpg"
            },
            new Vehicle
            {
                VehicleID =2,
                Type="Modernized Jeepney (Aircon)",
                Info=new VehicleInfo
                {
                    MinimumFare = 14.0,
                    MinimumKM = 4,
                    Comfortability = 8,
                    FareRate = 2.20
                },
                ImageFileName="Sample/modern_jeepney_sample.jpg"
            },
            new Vehicle
            {
                VehicleID =3,
                Type="MyBus",
                Info=new VehicleInfo
                {
                    MinimumFare = 30.0,
                    MinimumKM = 5,
                    Comfortability = 8,
                    FareRate = 2.10
                },
                ImageFileName="Sample/mybus_sample.jpg"
            }
        };
        public List<Vehicle> GetVehicles() => _vehicles;
        public Vehicle GetVehicleById(int vehicle_ID)
        {
            return _vehicles.FirstOrDefault(x => x.VehicleID == vehicle_ID);
        }

    }
}
