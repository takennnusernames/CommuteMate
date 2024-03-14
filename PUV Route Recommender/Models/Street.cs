using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Models
{
    public class Street
    {
        public int Way_Id { get; set; }
        public string Name { get; set; }
        public List<double> Nodes { get; set; }
    }
}
