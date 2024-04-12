using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Interfaces
{
    public interface IRouteStreetRepository
    {
        Task AddRouteStreetAsync(RouteStreet routeStreet);
        Task<IEnumerable<RouteStreet>> GetRouteStreetsAsync(int routeId);
        Task<IEnumerable<RouteStreet>> GetStreetRoutesAsync(int streetId);
    }
}
