using NetTopologySuite.Geometries;
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
    public class RoutePath
    {
        public int StreetId { get; set; }
        public int StreetRank { get; set; }
        public string StreetAction {  get; set; }
        public string PathLineString { get; set; }
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
    }
}
