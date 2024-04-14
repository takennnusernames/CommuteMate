namespace CommuteMate.Models
{
    public class PathAction
    {
        public Street Street { get; set; }
        public string Act { get; set; }
    }

    public class RoutePath
    {
        public List<PathAction> PathAction { get; set; }
        public Route puvRoute { get; set; }

    }
}
