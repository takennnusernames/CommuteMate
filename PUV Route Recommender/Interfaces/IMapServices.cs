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
        Task<Map> AddPin(Map map, LocationDetails location);
        Task<LocationDetails> GetCurrentLocationAsync();
        Task<List<LocationDetails>> SearchLocationAsync(string input);
        Task<List<LocationDetails>> GoogleSearchLocationAsync(string input);
        Task<ORSDirectionsDTO> GetDirectionsAsync(Coordinate origin, Coordinate destination);
        Task<List<PathData>> GetOptions(Feature feature);
        Task<Queue<List<(IEnumerable<Edge<Coordinate>>, RouteQueue)>>> GetRoutesQueue(List<Street> streets);
        string LineStringToWKT(LineString lineString);
        Task<Map> addLineString(Map map, string WKTString, string style);
        Task<Map> addLineString(Map map, List<string> WKTStrings);
        Task addPath(Map map, IEnumerable<Edge<Coordinate>> path, string style);
    }
}
