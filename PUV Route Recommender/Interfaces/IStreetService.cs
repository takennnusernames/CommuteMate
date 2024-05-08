using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IStreetService
    {
        Task<Street> InsertStreetAsync(Street street);
        Task<Street> GetStreetByWayIdAsync(long wayId);
        Task<int> GetStreetIdAsync(string name);
        Task<Street> GetStreetByIdAsync(int id);
        Task UpdateStreetAsync(Street street);
        Task<string> StreetListToWkt(List<Coordinate> coordinates);
        Task<List<Coordinate>> WktToStreetList(string wktString);
        Task<List<StreetWithNode>> StreetSequenceOrder(List<StreetWithNode> streets);
        Task<LinkedList<StreetWithCoordinates>> StartToEndStreets(long startId, long endId, List<StreetWithCoordinates> streets, long routeId);
        Task<List<long>> CheckNeighboringStreets(long relationId, Coordinate coordinates);
    }
}
