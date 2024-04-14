using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IRouteService
    {
        Task<Route> InsertRouteAsync(Route route);
        Task<List<Route>> GetAllRoutesAsync();
        Task<Route> GetRouteByOsmIdAsync(long id);
        Task<Route> GetRouteByIdAsync(int id);
        Task UpdateRouteAsync(Route route);
        Task<int> CountRoutesAsync();
    }
}
