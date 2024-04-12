using NetTopologySuite.Geometries;
using SQLite;
using SQLiteNetExtensions.Attributes;


namespace PUV_Route_Recommender.Models
{
    public class Route
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public long Osm_Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        [ManyToMany(typeof(RouteStreet))]
        public ICollection<Street> Streets { get; set; }
    }
    public class Street
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public long Way_Id { get; set; }
        public string Name { get; set; }
        public string GeometryWKT { get; set; }
        [ManyToMany(typeof(RouteStreet))]
        public ICollection<Route> Routes { get; set; }
    }
    public class RouteStreet
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [ForeignKey(typeof(Route))]
        public int RouteId { get; set; }
        [ForeignKey(typeof(Street))]
        public int StreetId { get; set; }
    }
}
