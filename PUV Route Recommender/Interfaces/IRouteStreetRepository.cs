using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IRouteStreetRepository
    {
        Task AddRouteStreetAsync(RoutePath routeStreet);
        //Task<IEnumerable<RoutePath>> GetRouteStreetsAsync(int routeId);
        //Task<IEnumerable<RoutePath>> GetStreetRoutesAsync(int streetId);
    }
}
