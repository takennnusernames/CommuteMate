using NetTopologySuite.Geometries;
using CommuteMate.DTO;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using Map = Mapsui.Map;
using GoogleMap = Microsoft.Maui.Controls.Maps.Map;
using QuickGraph;
using Microsoft.Maui.Controls.Maps;

namespace CommuteMate.Interfaces
{
    public interface IMapServices
    {
        Task<Map> CreateMapAsync();
        Task CreateGoogleMapAsync(GoogleMap map);
        Task<Map> AddPin(Map map, LocationDetails location);
        Task<Pin> AddGooglePin(Location location, GoogleMap map, string label);
        Task<Pin> AddGooglePin(LocationDetails location, GoogleMap map);
        Task RemoveGooglePin(Pin pin, GoogleMap map);
        Task AddGooglePolyline(Geometry geometry, GoogleMap map, string action);
        Task<LocationDetails> GetCurrentLocationAsync();
        Task<List<LocationDetails>> SearchLocationAsync(string input);
        Task<List<LocationDetails>> GoogleSearchLocationAsync(string input);
        Task<ORSDirectionsDTO> GetDirectionsAsync(Coordinate origin, Coordinate destination);
        Task<List<PathData>> GetOptions(Feature feature);
        Task<Queue<List<(IEnumerable<Edge<Coordinate>>, RouteQueue)>>> GetRoutesQueue(List<Street> streets);
        string LineStringToWKT(LineString lineString);
        Task<Map> addLineString(Map map, string WKTString, string style);
        Task<Map> addLineString(Map map, List<string> WKTStrings);
        Task<Map> addLineString(Map map, NetTopologySuite.Geometries.Geometry lineString, string style);
        Task addPath(Map map, IEnumerable<Edge<Coordinate>> path, string style);
        Task<Map> addGeometry(Map map, Geometry geometry, string style);
    }
}
