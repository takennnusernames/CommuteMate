using NetTopologySuite.Geometries;
namespace CommuteMate.Models
{
    public class RoutePath : BindableObject
    {
        public List<RouteStep> Steps { get; set; }
        public PathSummary Summary { get; set; }
        public List<Geometry> RouteGeometry { get; set; }
        private bool isDownloaded = false;
        public bool IsDownloaded
        {
            get { return isDownloaded; }
            set
            {
                isDownloaded = value;
                OnPropertyChanged(nameof(IsDownloaded));
            }
        }
        public bool IsNotDownloaded => !IsDownloaded;
    }
    public class RouteStep
    {
        public string Action { get; set; }
        public string Instruction { get; set; }
        public Geometry StepGeometry { get; set; }
    }
    public class PathSummary
    {
        public double TotalDistance { get; set; }
        public double TotalDuration { get; set; }
        public double TotalFare {  get; set; }
        public List<string> PUVCodes { get; set; }
    }
}
