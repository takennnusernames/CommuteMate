using CommuteMate.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Services
{
    public class RouteStreetService : IRouteStreetService
    {
        readonly IRouteStreetRepository _routeStreetRepository;
        readonly IStreetService _streetService;
        public RouteStreetService(IRouteStreetRepository routeStreetRepository, IStreetService streetService)
        {
            _routeStreetRepository = routeStreetRepository;
            _streetService = streetService;

        }
        public async Task<bool> CreateRelation(RouteStreet relation)
        {
            try
            {
                if (_routeStreetRepository.CheckRelation(relation.StreetOsmId,relation.RouteOsmId))
                    return true;
                await _routeStreetRepository.InsertStreetRelation(relation);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public async Task<List<Street>> GetRelatedStreets(long osmId)
        {
            try
            {
                return await _routeStreetRepository.GetRelatedStreets(osmId);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return [];
            }
        }
    }
}
