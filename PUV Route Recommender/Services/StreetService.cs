using CommuteMate.DTO;
using CommuteMate.Interfaces;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Services
{
    public class StreetService : IStreetService
    {
        HttpClient _httpClient;
        CancellationTokenSource _cancellationTokenSource;
        private readonly IStreetRepository _streetRepository;
        public StreetService(IStreetRepository streetRepository)
        {
            _httpClient = new HttpClient();
            _cancellationTokenSource = new CancellationTokenSource();
            _streetRepository = streetRepository;
        }
        public async Task<Street> InsertStreetAsync(Street street)
        {
            return await _streetRepository.InsertStreetAsync(street);
        }
        public async Task<Street> GetStreetByWayIdAsync(long wayId)
        {
            return await _streetRepository.GetStreetByOsmIdAsync(wayId);
        }
        public async Task<int> GetStreetIdAsync(string name)
        {
            return await _streetRepository.GetStreetIdAsync(name);
        }
        public async Task<Street> GetStreetByIdAsync(int wayId)
        {
            return await _streetRepository.GetStreetByIdAsync(wayId);
        }
        public async Task UpdateStreetAsync(Street street)
        {
            await _streetRepository.UpdateStreetAsync(street);
        }
    }
}
