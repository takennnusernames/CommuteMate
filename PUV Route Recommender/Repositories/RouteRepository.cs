using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using CommuteMate.Interfaces;
using SQLite;
using System.Collections;

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
                Console.WriteLine($"Failed to retrive routes: {ex.Message}");
                throw;
            }
        }

        //read
        public async Task<IEnumerable<Route>> GetAllRoutesAsync()
        {
            try
            {
                return await _dbContext.Routes.Include(r => r.Streets).ToListAsync();
                //if(await db.Table<Route>().CountAsync() != 0)
                //{
                //    var routes = await db.Table<Route>().ToListAsync();
                //    return routes;
                //}
                //Console.WriteLine($"Failed to retrive routes:");
                //throw new Exception("Route Table is empty");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrive routes: {ex.Message}");
                throw;
            }
        }

        public async Task<Route> GetRouteByOsmIdAsync(long id)
        {
            try
            {
                return await _dbContext.Routes.Where(r => r.Osm_Id == id).Include(r => r.Streets).FirstOrDefaultAsync();
                //return await db.Table<Route>().FirstOrDefaultAsync(r => r.Osm_Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve route: {ex.Message}");
                throw;
            }
        }

        public async Task<Route> GetRouteByIdAsync(int id)
        {
            var route = await _dbContext.Routes.Include(r => r.Streets).FirstOrDefaultAsync(r => r.RouteId == id);
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
            _dbContext.Entry(route).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
    }
    
}
