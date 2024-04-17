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
        public async Task<Route> InsertRouteAsync(Route route)
        {
            return await _routeRepository.InsertRouteAsync(route);
        }
        public async Task<List<Route>> GetAllRoutesAsync()
        {
            var routes = await _routeRepository.GetAllRoutesAsync();
            return routes.ToList();
        }
        public async Task<List<Street>> GetRouteStreets(int id)
        {
            var streets = await _routeRepository.GetRouteStreets(id);
            return streets.ToList();
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
        public async Task<int> CountRoutesAsync()
        {
            return await _routeRepository.CountRoutesAsync();
        }
    }
}
