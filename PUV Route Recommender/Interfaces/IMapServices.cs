using NetTopologySuite.Geometries;
using CommuteMate.DTO;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using Map = Mapsui.Map;

namespace CommuteMate.Interfaces
{
    public interface IMapServices
    {
        Task<Map> CreateMapAsync();
        Task<Location> GetLocationAsync(string location);
        Task<List<string>> SearchLocationAsync(string input);
        Task GetDirectionsAsync(double origin, double destination);
        string LineStringToWKT(LineString lineString);
        string StreetListToWkt(List<Coordinate> coordinates);
    }
}
