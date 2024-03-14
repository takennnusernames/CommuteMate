using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Interfaces
{
    public interface IRouteService
    {
        Task<List<Route>> GetRoutesAsync();
    }
}
