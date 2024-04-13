using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.DTO
{
    public class ORSDirectionsDTO
    {
        public string type { get; set; }
        public Metadata metadata { get; set; }
        public List<double> bbox { get; set; }
        public List<Feature> features { get; set; }
    }
    public class AlternativeRoutes
    {
        public int target_count { get; set; }
        public double weight_factor { get; set; }
        public double share_factor { get; set; }
    }

    public class Engine
    {
        public string version { get; set; }
        public DateTime build_date { get; set; }
        public DateTime graph_date { get; set; }
    }

    public class Extras
    {
        public Roadaccessrestrictions roadaccessrestrictions { get; set; }
    }

    public class Feature
    {
        public List<double> bbox { get; set; }
        public string type { get; set; }
        public Properties properties { get; set; }
        public ORSGeometry geometry { get; set; }
    }

    public class ORSGeometry
    {
        public List<Coordinate> coordinates { get; set; }
        public string type { get; set; }
    }

    public class Metadata
    {
        public string attribution { get; set; }
        public string service { get; set; }
        public long timestamp { get; set; }
        public Query query { get; set; }
        public Engine engine { get; set; }
    }

    public class Properties
    {
        public int transfers { get; set; }
        public int fare { get; set; }
        public List<Segment> segments { get; set; }
        public List<Warning> warnings { get; set; }
        public List<int> way_points { get; set; }
        public Summary summary { get; set; }
        public Extras extras { get; set; }
    }

    public class Query
    {
        public List<Coordinate> coordinates { get; set; }
        public string profile { get; set; }
        public string format { get; set; }
        public List<string> extra_info { get; set; }
        public AlternativeRoutes alternative_routes { get; set; }
    }

    public class Roadaccessrestrictions
    {
        public List<List<int>> values { get; set; }
        public List<Summary> summary { get; set; }
    }

    

    public class Segment
    {
        public double distance { get; set; }
        public double duration { get; set; }
        public List<Step> steps { get; set; }
    }

    public class Step
    {
        public double distance { get; set; }
        public double duration { get; set; }
        public int type { get; set; }
        public string instruction { get; set; }
        public string name { get; set; }
        public List<int> way_points { get; set; }
    }

    public class Summary
    {
        public double distance { get; set; }
        public double duration { get; set; }
        public double value { get; set; }
        public double amount { get; set; }
    }

    public class Warning
    {
        public int code { get; set; }
        public string message { get; set; }
    }
}
