using NetTopologySuite.Geometries;
using QuickGraph;
using SQLite;
using System.ComponentModel.DataAnnotations.Schema;


namespace CommuteMate.Models
{
    //db tables
    public class Route
    {
        [PrimaryKey, AutoIncrement]
        public int RouteId { get; set; }
        [Unique]
        public long Osm_Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public bool StreetNameSaved { get; set; }
        public virtual ICollection<Street> Streets { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var otherRoute = (Route)obj;
            return RouteId == otherRoute.RouteId && Osm_Id == otherRoute.Osm_Id;
            // Add other relevant property comparisons if needed
        }

        public override int GetHashCode()
        {
            return RouteId.GetHashCode() ^ Osm_Id.GetHashCode();
            // Combine hash codes of relevant properties
        }
    }
    public class Street
    {
        [PrimaryKey, AutoIncrement]
        public int StreetId { get; set; }
        [Unique]
        public long Osm_Id { get; set; }
        public string Name { get; set; }
        public string GeometryWKT { get; set; }

        [NotMapped]
        public List<Coordinate> Coordinates { get; set; }
        public virtual ICollection<Route> Routes { get; set; }
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var otherStreet = (Street)obj;
            return Routes == otherStreet.Routes;
            // Add other relevant property comparisons if needed
        }

        public override int GetHashCode()
        {
            return StreetId.GetHashCode() ^ Osm_Id.GetHashCode();
            // Combine hash codes of relevant properties
        }
    }

    //project objects
    public class PathData
    {
        public List<Street> streets {  get; set; }
        public Queue<Route> puvs { get; set; }
        public IEnumerable<Edge<Coordinate>> puvShortestPaths { get; set; }
        public IEnumerable<Edge<Coordinate>> walkingPath { get; set; }
        public double totalFare { get; set; }
        public double totalWalkingDistance { get; set; }
        public double totalPuvRideDistance { get; set; }

    }

    public class RouteAction
    {
        public Street street { get; set; }
        public string action { get; set; }
    }

    public class RouteQueueInfo
    {
        public Route route { get; set; }
        public int intersects { get; set; }
        public Street Start { get; set; }
        public Street End { get; set; }
    }
    public class StreetWithNode
    {
        public int StreetId { get; set; }
        public long Osm_Id { get; set; }
        public string Name { get; set; }
        public List<long> Nodes { get; set; }
        public string NodesAsString => string.Join(", ", Nodes);
    }
    public class StreetWithCoordinates
    {
        public int StreetId { get; set; }
        public long Osm_Id { get; set; }
        public string Role { get; set; }
        public List<Coordinate> Coordinates { get; set; }
    }

    public class RouteQueue
    {
        public Route route { get; set; }
        public int intersectCount { get; set; }
        public Street startStreet { get; set; }
        public Street endStreet { get; set; }
    }
}
