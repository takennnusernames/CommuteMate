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
        public virtual ICollection<Street> Streets { get; set; }
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
    }
    public class RouteStreet
    {
        public int RouteId { get; set; }
        [Ignore]
        public Route Route { get; set; }
        public int StreetId { get; set; }
        [Ignore]
        public Street Street { get; set; }
    }
}
