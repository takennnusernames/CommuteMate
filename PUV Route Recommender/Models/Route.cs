using NetTopologySuite.Geometries;
using SQLite;
using System.ComponentModel.DataAnnotations.Schema;


namespace CommuteMate.Models
{
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
    public class RoutePath
    {
        public int StreetId { get; set; }
        public int StreetRank { get; set; }
        public string StreetAction {  get; set; }
        public string PathLineString { get; set; }
    }
    public class StreetOrder
    {
        public Street Street { get; set; }
        public int Rank { get; set;}
    }
}
