using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Models
{
    public class Vehicle
    {
        public int VehicleID { get; set; }
        public string Type { get; set; }
        public VehicleInfo Info { get; set; }
        public string ImageFileName { get; set; }
    }
    public class VehicleInfo
    {
        public double MinimumFare { get; set; }
        public double MinimumKM { get; set; }
        public double FareRate { get; set; }
        public double Comfortability { get; set; }

    }
}
