using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Models
{
    public class Route
    {
        public long Osm_Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set;}
        public List<double> Streets { get; set;}
    }
}
