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

        public async void InsertStreetRelation(RouteStreet routeStreet)
        {
            try
            {
                await _dbContext.AddAsync(routeStreet);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to insert route: {ex.Message}");
                throw;
            }
        }
    }
}
