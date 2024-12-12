using NetTopologySuite.Geometries;

namespace CommuteMate.Interfaces
{
    public interface ICommuteMateApiService
    {
        Task<List<Route>> GetRoutes();
        Task<List<Street>> GetRouteStreets(long osmId);
        Task<List<RoutePath>> GetPath(Coordinate origin, Coordinate destination);
        Task<List<string>> SearchRoute(string text);
        Task<List<Vehicle>> GetVehicles();
    }
}
