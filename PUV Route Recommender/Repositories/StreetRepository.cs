using Microsoft.Maui.Controls;
using PUV_Route_Recommender.Interfaces;
using PUV_Route_Recommender.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Repositories
{
    public class StreetRepository : IStreetRepository
    {
        private readonly SQLiteAsyncConnection db;
        public StreetRepository(SQLiteAsyncConnection db)
        {
            this.db = db;
        }
        public async Task<int> InsertStreetAsync(Street street)
        {
            try
            {
                await db.InsertAsync(street);
                return street.Id;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add route: {ex.Message}");
                throw;
            }
        }
        public async Task<Street> GetStreetByWayIdAsync(long wayId)
        {
            return await db.Table<Street>().FirstOrDefaultAsync(x => x.Way_Id == wayId);
        }

        public async Task<Street> GetStreetByIdAsync(int id)
        {
            return await db.Table<Street>().FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<int> GetStreetIdAsync(string name)
        {
            Street street = await db.Table<Street>().FirstOrDefaultAsync(x => x.Name == name);
            return street.Id;
        }
    }
}
