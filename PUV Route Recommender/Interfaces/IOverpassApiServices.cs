using CommuteMate.DTO;
using CommuteMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IOverpassApiServices
    {
        Task RetrieveOverpassRoutesAsync();
        Task RetrieveOverpassRouteStreetNamesAsync(long wayId, int routeId);
        Task<Street> RetrieveOverpassStreetAsync(long OsmId);
        Task RetrieveOverpassRouteStreetsAsync();
    }
}
