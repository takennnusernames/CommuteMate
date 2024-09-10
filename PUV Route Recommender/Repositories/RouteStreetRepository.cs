using CommuteMate.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Repositories
{
    public class RouteStreetRepository : IRouteStreetRepository
    {
        private readonly CommuteMateDbContext _dbContext;
        public RouteStreetRepository(CommuteMateDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> InsertStreetRelation(RouteStreet routeStreet)
        {
            try
            {
                await _dbContext.AddAsync(routeStreet);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to insert relation: {ex.Message}");
                return false;
            }
        }

        public async Task<List<Street>> GetRelatedStreets(long osmId)
        {
            return await Task.FromResult(_dbContext.Streets.Where(street => street.RouteStreets.Any(routeStreet => routeStreet.RouteOsmId == osmId)).ToList());
        }
        public bool CheckRelation(long streetId, long routeId)
        {
            return _dbContext.RouteStreets.Any(r => r.RouteOsmId == routeId && r.StreetOsmId == streetId);
        }
    }
}
