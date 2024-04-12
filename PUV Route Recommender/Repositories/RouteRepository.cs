using NetTopologySuite.Geometries;
using PUV_Route_Recommender.Interfaces;
using SQLite;
using System.Collections;

namespace PUV_Route_Recommender.Repositories
{
    public class RouteRepository : IRouteRepository
    {
        private readonly SQLiteAsyncConnection db; 
        public RouteRepository(SQLiteAsyncConnection db)
        {
            this.db = db;
        }
        //create
        public async Task<int> InsertRouteAsync(Route route)
        {
            await db.InsertAsync(route);
            return route.Id;
        }

        //read
        public async Task<IEnumerable<Route>> GetAllRoutesAsync()
        {
            try
            {
                if(await db.Table<Route>().CountAsync() != 0)
                {
                    var routes = await db.Table<Route>().ToListAsync();
                    return routes;
                }
                Console.WriteLine($"Failed to retrive routes:");
                throw new Exception("Route Table is empty");
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
                return await db.Table<Route>().FirstOrDefaultAsync(r => r.Osm_Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve route: {ex.Message}");
                throw;
            }
        }

        public async Task<Route> GetRouteByIdAsync(int id)
        {
            try
            {
                return await db.Table<Route>().FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrieve route: {ex.Message}");
                throw;
            }
        }

        public async Task<int> CountRoutesAsync()
        {
            try
            {
                return await db.Table<Route>().CountAsync();
            }
            catch (Exception ex)
            {
                // Handle the exception (e.g., log it, rethrow it, etc.)
                Console.WriteLine($"An error occurred while counting routes: {ex.Message}");
                throw;
            }
        }

        //public async Task<IEnumerable<Route>> GetRoutesByWayId(long wayId)
        //{
        //    try
        //    {
        //        return await db.Table<Route>().Where(r => r.Streets.Any(s => s.Way_Id == wayId)).ToListAsync();
            
        //         //return _routes.Where(route => route.Streets.Any(street => street.Way_Id == wayId));
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred while getting routes: {ex.Message}");
        //        throw;
        //    }
        //}

    }
    
}
