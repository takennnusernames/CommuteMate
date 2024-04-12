using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Models
{
    public class RouteInfo
    {
        public Route Route { get; set; }
        public List<Street> Street { get; set; }
        public List<string> StreetNames { get; set; }
    }
}
