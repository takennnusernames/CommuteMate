using CommuteMate.DTO;
using CommuteMate.Models;
using NetTopologySuite.Geometries;
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
        Task<List<Route>> RetrieveRelatedRoutes(long OsmId);
        Task<List<Street>> RetrieveRelatedStreetsAsync(long OsmId);
        Task<List<StreetWithNode>> RetrieveStreetWithNodesAsync(long OsmId);
        Task<List<StreetWithCoordinates>> RetrieveStreetWithCoordinatesAsync(long OsmId);
        Task<List<(Street, Coordinate)>> GeometryToStreetListAsync(DTO.Geometry geometry);
    }
}
