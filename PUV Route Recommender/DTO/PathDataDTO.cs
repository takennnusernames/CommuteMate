using Newtonsoft.Json;
using Geometry = NetTopologySuite.Geometries.Geometry;
using CommuteMate.Utilities;

namespace CommuteMate.DTO
{
    public class PathDataDTO
    {
        public List<PathFeatures> features {  get; set; }
    }   
    public class PathFeatures
    {
        public Property properties { get; set; } = new();
        public List<Geometry> geometry { get; set; }
    }
    //public class Geometry
    //{
    //    public List<List<object>> coordinates { get; set; }
    //    public string type { get; set; }
    //}
    public class Property
    {
        public List<Segment> segments { get; set; } = new();
        public Summary summary { get; set; } = new();
    }
    public class Summary
    {
        public double distance { get; set; }
        public double duration { get; set; }
        public double fare { get; set; }
    }
    public class Segment
    {
        public string type { get; set; } = "";
        public double distance { get; set; }
        public double duration { get; set; }
        public List<Step> steps { get; set; } = new();
    }
    public class Step
    {
        public double distance { get; set; }
        public double duration { get; set; }
        public string code { get; set; } = "";
        public double fare { get; set; }
        public string instruction { get; set; } = "";
        public string from { get; set; } = "";
        public string to { get; set; } = "";
        public Geometry geometry { get; set; }
    }
    public class StreetGeometry
    {
        public Street Street { get; set; }
        public PathCoordinate Coordinate { get; set; }
    }
    public class PathCoordinate
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public PathCoordinate(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }
    }
    public class PathRequest
    {
        public PathCoordinate Origin { get; set; }
        public PathCoordinate Destination { get; set; }
    }
}
