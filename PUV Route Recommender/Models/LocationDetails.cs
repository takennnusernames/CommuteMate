using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Models
{
    public class LocationDetails
    {
        public string Name { get; set; }
        public Coordinate Coordinate { get; set; }
    }
}
