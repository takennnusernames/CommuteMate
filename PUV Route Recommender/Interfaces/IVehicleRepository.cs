using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IVehicleRepository
    {
        Vehicle GetVehicleById(int vehicle_ID);
        List<Vehicle> GetVehicles();
    }
}
