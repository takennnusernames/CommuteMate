using NetTopologySuite.Geometries;
namespace CommuteMate.Models
{
    public class PathAction
    {
        public Street Street { get; set; }
        public string Act { get; set; }
    }
    public class RoutePath
    {
        public List<RouteStep> Steps { get; set; }
        public PathSummary Summary { get; set; }
        public List<Geometry> RouteGeometry { get; set; }
    }
    public class RouteStep
    {
        public string Action { get; set; }
        public string Instruction { get; set; }
        public Geometry StepGeometry { get; set; }
    }
    public class PathSummary
    {
        public string TotalDistance { get; set; }
        public string TotalDuration { get; set; }
        public string TotalFare {  get; set; }
        public List<string> PUVCodes { get; set; }
    }
}
