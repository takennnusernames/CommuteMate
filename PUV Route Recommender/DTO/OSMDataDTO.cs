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
    public class Element
    {
        public string type { get; set; }
        public long id { get; set; }
        public Dictionary<string, string> tags { get; set; }
        public List<OSMCoordinate> geometry { get; set; }
    }
    public class BoundingBox
    {
        public double minLat { get; set; }
        public double minLon { get; set; }
        public double maxLat { get; set; }
        public double maxLon { get; set; }
    }

    public class OSMCoordinate
    {
        public double lat { get; set; }
        public double lon { get; set; }
    }
    public class OSMDataDTO
    {
        public float version { get; set; }
        public string generator { get; set; }
        public OSM3S osm3s { get; set; }
        public List<Element> elements { get; set; }

    }
}
