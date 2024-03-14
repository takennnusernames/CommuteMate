using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.DTO
{
    public class OSM3S
    {
        public DateTime timestamp_osm_base { get; set; }
        public DateTime timestamp_areas_base { get; set; }
        public string copyright { get; set; }
    }
    public class OSMRelationDTO
    {
        public string type { get; set; }
        public long id { get; set; }
        public List<OSMWayDTO> members { get; set; }
        public Dictionary<string, string> tags { get; set; }
    }
    public class OSMWayDTO
    {
        public string type { get; set; }
        public object @ref { get; set; }
        public string role {  get; set; }
    }
    public class OSMDataDTO
    {
        public float version { get; set; }
        public string generator { get; set; }
        public OSM3S osm3s { get; set; }
        public List<OSMRelationDTO> elements { get; set; }

    }
}
