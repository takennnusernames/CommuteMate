using Microsoft.Maui.Controls;
using CommuteMate.Interfaces;
using CommuteMate.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CommuteMate.Repositories
{
    public class StreetRepository : IStreetRepository
    {
        private readonly CommuteMateDbContext _dbContext;
        public StreetRepository(CommuteMateDbContext db)
        {
            _dbContext = db;
        }
        public async Task<int> InsertStreetAsync(Street street)
        {
            try
            {
                await _dbContext.AddAsync(street);
                await _dbContext.SaveChangesAsync();
                return street.StreetId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add route: {ex.Message}");
                throw;
            }
        }
        public async Task<Street> GetStreetByWayIdAsync(long wayId)
        {

            return await _dbContext.Streets.Where(s => s.Way_Id == wayId).FirstOrDefaultAsync();
            //return await db.Table<Street>().FirstOrDefaultAsync(x => x.Way_Id == wayId);
        }

        public async Task<Street> GetStreetByIdAsync(int id)
        {
            return await _dbContext.Streets.Where(s => s.StreetId == id).FirstOrDefaultAsync();
            //return await db.Table<Street>().FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<int> GetStreetIdAsync(string name)
        {
            Street street = await _dbContext.Streets.Where(s => s.Name == name).FirstOrDefaultAsync();
            return street.StreetId;
        }
    }
}
