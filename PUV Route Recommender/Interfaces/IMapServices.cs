using NetTopologySuite.Geometries;
using CommuteMate.DTO;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using Map = Mapsui.Map;
using QuickGraph;

namespace CommuteMate.Interfaces
{
    public interface IMapServices
    {
        Task<Map> CreateMapAsync();
        Task<Location> GetLocationAsync(string location);
        Task<List<string>> SearchLocationAsync(string input);
        Task<ORSDirectionsDTO> GetDirectionsAsync(Coordinate origin, Coordinate destination);
        Task<List<PathData>> GetOptions(Feature feature);
        Task<Queue<List<RouteQueue>>> GetRoutesQueue(List<Street> streets);
        string LineStringToWKT(LineString lineString);
        Task<Map> addLineString(Map map, string WKTString);
    }
}
