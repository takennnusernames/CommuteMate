using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IRouteStreetService
    {
        Task<bool> CreateRelation(RouteStreet relation);
        Task<List<Street>> GetRelatedStreets(long osmId);
    }
}
