using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.DTO
{
    public class RouteDTO
    {
        public int Id { get; init; }
        public string RouteName { get; init; }
        public string RouteCode { get; init; }
        public long OsmId { get; init; }
    }

    public class GeometryDTO
    {
        public string Type { get; set; }
        public List<List<double>> Coordinates { get; set; }
    }

    public class StreetDTO
    {
        public int Id { get; set; }
        public int OsmId { get; set; }
        public string StreetName { get; set; }
        public string GeometryWKT { get; set; }
    }
}
