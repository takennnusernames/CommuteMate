﻿using Microsoft.Maui.Controls;
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
        public async Task<Street> InsertStreetAsync(Street street)
        {
            try
            {
                if (_dbContext.Streets.Where(s => s.OsmId == street.OsmId).FirstOrDefault() is not null)
                    return street;
                await _dbContext.AddAsync(street);
                await _dbContext.SaveChangesAsync();
                return street;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add street: {ex.Message}");
                throw;
            }
        }
        public async Task<Street> GetStreetByOsmIdAsync(long osmId)
        {

            return await _dbContext.Streets.Where(s => s.OsmId == osmId).Include(s => s.Routes).FirstOrDefaultAsync();
            //return await db.Table<Street>().FirstOrDefaultAsync(x => x.Way_Id == wayId);
        }

        public async Task<List<Street>> GetStreetByRouteIdAsync(long osmId)
        {
            var streets = _dbContext.Streets.Where(s => s.OsmId == osmId).ToList();
            return await Task.FromResult(streets);
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
        public async Task UpdateStreetAsync(Street street)
        {
            _dbContext.Entry(street).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();
        }
        public async Task DeleteStreetAsync(Street street)
        {
            _dbContext.Streets.Remove(street);
            await _dbContext.SaveChangesAsync();
        }
    }
}
