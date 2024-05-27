using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IRouteRepository
    {
        Task<Route> InsertRouteAsync(Route route);
        Task<List<Route>> GetAllRoutesAsync();
        Task<IEnumerable<Street>> GetRouteStreets(int id);
        Task<Route> GetRouteByIdAsync(int id);
        Task<int> CountRoutesAsync();
        Task<Route> GetRouteByOsmIdAsync(long id);
        Task UpdateRouteAsync(Route route);
    }
}
