using PUV_Route_Recommender.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Repositories
{
    public class RouteStreetRepository : IRouteStreetRepository
    {
        private readonly SQLiteAsyncConnection db;
        public RouteStreetRepository(SQLiteAsyncConnection db) 
        {
            this.db = db;
        }
        public async Task AddRouteStreetAsync(RouteStreet routeStreet)
        {
            await db.InsertAsync(routeStreet);
        }

        public async Task<IEnumerable<RouteStreet>> GetRouteStreetsAsync(int routeId)
        {
            var query = db.Table<RouteStreet>().Where(r => r.RouteId == routeId);
            return await query.ToListAsync();
        }
        public async Task<IEnumerable<RouteStreet>> GetStreetRoutesAsync(int streetId)
        {
            return await db.Table<RouteStreet>().Where(r => r.StreetId == streetId).ToListAsync();
        }
    }
}
