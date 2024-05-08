using NetTopologySuite.Geometries;
using QuickGraph;
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
        Task<List<Street>> GetRouteStreets(int id);
        Task UpdateRouteAsync(Route route);
        Task<int> CountRoutesAsync();
        Task<UndirectedGraph<Coordinate, Edge<Coordinate>>> StreetToGraph(List<StreetWithCoordinates> streets, long routeId);
        UndirectedGraph<Coordinate, Edge<Coordinate>> StreetToGraph(List<Street> streets);
        IEnumerable<Edge<Coordinate>> GetShortetstPath(UndirectedGraph<Coordinate, Edge<Coordinate>> graph, Coordinate origin, Coordinate destination);


    }
}
