using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Interfaces
{
    public interface IRouteService
    {
        Task<int> InsertRouteAsync(Route route);
        Task<List<Route>> GetAllRoutesAsync();
        Task<Route> GetRouteByOsmIdAsync(long id);
        Task<Route> GetRouteByIdAsync(int id);
    }
}
