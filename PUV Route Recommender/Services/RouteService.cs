using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Linemerge;
using CommuteMate.Interfaces;

namespace CommuteMate.Services
{
    public class RouteService : IRouteService
    {
        IRouteRepository _routeRepository;

        public RouteService(IRouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }
        public async Task<int> InsertRouteAsync(Route route)
        {
            return await _routeRepository.InsertRouteAsync(route);
        }
        public async Task<List<Route>> GetAllRoutesAsync()
        {
            var routes = await _routeRepository.GetAllRoutesAsync();
            return routes.ToList();
        } 

        public async Task<Route> GetRouteByIdAsync(int id)
        {
            return await _routeRepository.GetRouteByIdAsync(id);
        }
        public async Task<Route> GetRouteByOsmIdAsync(long id)
        {
            return await _routeRepository.GetRouteByOsmIdAsync(id);
        }
        public async Task UpdateRouteAsync(Route route)
        {
            await _routeRepository.UpdateRouteAsync(route);
        }
    }
}
