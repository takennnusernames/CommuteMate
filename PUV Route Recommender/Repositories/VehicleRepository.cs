using PUV_Route_Recommender.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Repositories
{
    public static class VehicleRepository
    {
        public static List<Vehicle> _vehicles = new List<Vehicle>()
        {
            new Vehicle { Vehicle_ID =0, Type="Jeepney", Vehicle_Code="PUV" , Route="testRoute"},
            new Vehicle { Vehicle_ID =1, Type="Modernized Jeepney", Vehicle_Code="PUV", Route="testRoute"},
            new Vehicle { Vehicle_ID =2, Type="CiBus", Vehicle_Code="PUV", Route="testRoute"},
            new Vehicle { Vehicle_ID =3,Type="MyBus", Vehicle_Code="PUV", Route="testRoute"}
        };
        public static List<Vehicle> GetVehicles() => _vehicles;
        public static Vehicle GetVehicleById(int vehicle_ID)
        {
            return _vehicles.FirstOrDefault(x => x.Vehicle_ID == vehicle_ID);
        }

    }
}
