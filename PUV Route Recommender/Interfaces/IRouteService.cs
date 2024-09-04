using NetTopologySuite.Geometries;

namespace CommuteMate.Interfaces
{
    public interface IRouteService
    {
        Task<Route> InsertRouteAsync(Route route);
        Task<List<Route>> GetAllRoutesAsync();
        Task<List<Route>> GetOfflineRoutesAsync();
        Task<Route> GetRouteByOsmIdAsync(long id);
        Task<Route> GetRouteByIdAsync(int id);
        Task<List<Street>> GetRouteStreets(int id);
        Task UpdateRouteAsync(Route route);
        Task<int> CountRoutesAsync();

    }
}
