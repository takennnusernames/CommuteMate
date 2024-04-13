using CommuteMate.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Services
{
    public class RouteStreetService : IRouteStreetService
    {
        private readonly IRouteStreetRepository _routeStreetRepository;
        private readonly IStreetService _streetService;
        private readonly IRouteService _routeService;
        public RouteStreetService(IRouteStreetRepository routeStreetRepository, IStreetService streetService, IRouteService routeService) 
        { 
            _routeStreetRepository = routeStreetRepository; 
            _streetService = streetService;
            _routeService = routeService;
        }
        public async Task AddRouteStreetAsync(int routeId, int streetId)
        {
            var routeStreet = new RouteStreet
            {
                RouteId = routeId,
                StreetId = streetId
            };
            await _routeStreetRepository.AddRouteStreetAsync(routeStreet);
        }
        //public async Task<List<Street>> GetRouteStreetsAsync(int routeId)
        //{
        //    try
        //    {
        //        var routeStreets = await _routeStreetRepository.GetRouteStreetsAsync(routeId);
        //        List<Street> streets = [];
        //        foreach (var routeStreet in routeStreets)
        //        {
        //            Street street = await _streetService.GetStreetByIdAsync(routeStreet.StreetId);
        //            if (street != null)
        //                streets.Add(street);
        //        }
        //        return streets;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Error!", ex.Message);
        //        throw new Exception("error unable to retrieve streets");
        //    }
        //}
        //public async Task<List<Route>> GetStreetRoutesAsync(int streetId)
        //{
        //    var routeStreets = await _routeStreetRepository.GetStreetRoutesAsync(streetId);
        //    List<Route> routes = [];
        //    foreach (var routeStreet in routeStreets)
        //    {
        //        Route route = await _routeService.GetRouteByIdAsync(routeStreet.RouteId);
        //        if (routes != null)
        //            routes.Add(route);
        //    }
        //    return routes;
        //}
    }
}
