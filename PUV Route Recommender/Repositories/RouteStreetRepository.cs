using Microsoft.EntityFrameworkCore;
using CommuteMate.Interfaces;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Repositories
{
    public class RouteStreetRepository : IRouteStreetRepository
    {
        private readonly CommuteMateDbContext db;
        public RouteStreetRepository(CommuteMateDbContext db) 
        {
            this.db = db;
        }
        public async Task AddRouteStreetAsync(RoutePath routeStreet)
        {
            await db.AddAsync(routeStreet);
        }

        //public async Task<IEnumerable<RoutePath>> GetRouteStreetsAsync(int routeId)
        //{
        //    using(var db = new CommuteMateDbContext())
        //    {
        //        db.Set<RoutePath>().Where(r => r.RouteId == routeId).Include(r => r.Street);
        //    }
        //    db.Set<RoutePath>().Where(r => r.RouteId == routeId).Include(r => r.Street);
        //    return await query.ToListAsync();
        //}
        //public async Task<IEnumerable<RoutePath>> GetStreetRoutesAsync(int streetId)
        //{
        //    return await db.Table<RoutePath>().Where(r => r.StreetId == streetId).ToListAsync();
        //}
    }
}
