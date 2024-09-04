using NetTopologySuite.Geometries;
using CommuteMate.Interfaces;

namespace CommuteMate.Services
{
    public class RouteService : IRouteService
    {
        IRouteRepository _routeRepository;
        private readonly IStreetService _streetService;

        public RouteService(IRouteRepository routeRepository, IStreetService streetService)
        {
            _routeRepository = routeRepository;
            _streetService = streetService;
        }
        public async Task<Route> InsertRouteAsync(Route newRoute)
        {
            var route = await _routeRepository.GetRouteByIdAsync(newRoute.RouteId);
            if (route is null)
                return await _routeRepository.InsertRouteAsync(newRoute);
            else
                return route;
        }
        public async Task<List<Route>> GetAllRoutesAsync()
        {
            var routes = await _routeRepository.GetAllRoutesAsync();
            if (routes is not null)
                return routes;
            return null;
        }
        public async Task<List<Route>> GetOfflineRoutesAsync()
        {
            var routes = await _routeRepository.GetAllRoutesAsync();

            if (routes is not null)
            {
                routes = routes.Where(route => route.StreetNameSaved == true).ToList();
                return routes;
            }
            return null;
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
