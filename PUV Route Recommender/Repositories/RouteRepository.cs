using Microsoft.EntityFrameworkCore;
using CommuteMate.Interfaces;

namespace CommuteMate.Repositories
{
    public class RouteRepository : IRouteRepository
    {
        private readonly CommuteMateDbContext _dbContext;
        public RouteRepository(CommuteMateDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        //create
        public async Task<Route> InsertRouteAsync(Route route)
        {
            try
            {
                await _dbContext.AddAsync(route);
                await _dbContext.SaveChangesAsync();
                return route;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to insert route: {ex.Message}");
                throw;
            }
        }
        //read
        public async Task<List<Route>> GetAllRoutesAsync()
        {
            try
            {
                var routes = _dbContext.Routes.ToList();
                if(routes is not null)
                    return await Task.FromResult(routes);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrive routes: {ex.Message}");
                throw;
            }
        }
        public async Task<IEnumerable<Street>> GetRouteStreets(int id)
        {
            try
            {
                var route = await _dbContext.Routes.Where(r => r.RouteId == id).Include(r => r.Streets).FirstOrDefaultAsync();
                return route.Streets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrive route: {ex.Message}");
                throw;
            }
        }

        public async Task<Route> GetRouteByOsmIdAsync(long id)
        {
            try
            {
                return await _dbContext.Routes.Where(r => r.OsmId == id).Include(r => r.Streets).FirstOrDefaultAsync();
                //return await db.Table<Route>().FirstOrDefaultAsync(r => r.OsmId == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve route: {ex.Message}");
                throw;
            }
        }

        public async Task<Route> GetRouteByIdAsync(int id)
        {
            var route = await _dbContext.Routes.FindAsync(id);
            if (route == default)
                return null;
            return route;
                //try
                //{
                //    return await db.Table<Route>().FirstOrDefaultAsync(r => r.Id == id);
                //}
                //catch (Exception ex)
                //{
                //    Console.WriteLine($"Failed to retrieve route: {ex.Message}");
                //    throw;
                //}
        }

        public async Task<int> CountRoutesAsync()
        {
            try
            {
                return await _dbContext.Routes.CountAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                Console.WriteLine($"An error occurred while counting routes: {ex.Message}");
                throw;
            }
        }

        public async Task UpdateRouteAsync(Route route)
        {
            //_dbContext.Routes.Update(route);
            _dbContext.Entry(route).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
    
}
