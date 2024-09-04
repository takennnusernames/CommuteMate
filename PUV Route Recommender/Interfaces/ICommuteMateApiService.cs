using CommuteMate.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace CommuteMate.Interfaces
{
    public interface ICommuteMateApiService
    {
        Task<List<Route>> GetRoutes();
        Task<List<Street>> GetRouteStreets(long osmId);
        Task<List<RoutePath>> GetPath(Coordinate origin, Coordinate destination);
        Task<List<string>> SearchRoute(string text);
    }
}
