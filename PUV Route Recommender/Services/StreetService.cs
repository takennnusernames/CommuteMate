using CommuteMate.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Services
{
    public class StreetService : IStreetService
    {
        private readonly IStreetRepository _streetRepository;
        public StreetService(IStreetRepository streetRepository) 
        {
            _streetRepository = streetRepository;
        }
        public async Task<int> InsertStreetAsync(Street street)
        {
            return await _streetRepository.InsertStreetAsync(street);
        }
        public async Task<Street> GetStreetByWayIdAsync(long wayId)
        {
            return await _streetRepository.GetStreetByWayIdAsync(wayId);
        }
        public async Task<int> GetStreetIdAsync(string name)
        {
            return await _streetRepository.GetStreetIdAsync(name);
        }
        public async Task<Street> GetStreetByIdAsync(int wayId)
        {
            return await _streetRepository.GetStreetByIdAsync(wayId);
        }
    }
}
