using NetTopologySuite.Geometries;
using PUV_Route_Recommender.DTO;
using Location = Microsoft.Maui.Devices.Sensors.Location;
using Map = Mapsui.Map;

namespace PUV_Route_Recommender.Interfaces
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
