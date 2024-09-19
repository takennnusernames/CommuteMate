using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.DTO
{
    public class PuvDTO
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public double MinimumFare { get; set; }
        public double MinimumKm { get; set; }
        public double FareIncrease { get; set; }
        public int Comfortability { get; set; }
        public string SampleImage { get; set; }
    }
}
