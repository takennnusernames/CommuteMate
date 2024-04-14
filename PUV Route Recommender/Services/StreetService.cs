using CommuteMate.Interfaces;
using NetTopologySuite.Geometries;
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

        public string StreetListToWkt(List<Coordinate> coordinates)
        {
            if (coordinates == null || coordinates.Count == 0)
            {
                return string.Empty;
            }

            // Construct the LINESTRING WKT string
            StringBuilder wktBuilder = new StringBuilder();
            wktBuilder.Append("LINESTRING (");

            foreach (Coordinate coord in coordinates)
            {
                wktBuilder.Append(coord.X).Append(" ").Append(coord.Y).Append(", ");
            }

            // Remove the trailing comma and space
            wktBuilder.Length -= 2;
            wktBuilder.Append(")");

            return wktBuilder.ToString();
        }
    }
}
