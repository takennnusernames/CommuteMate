using CommuteMate.Interfaces;
using NetTopologySuite.Geometries;
using System.Text;

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

        public async Task<List<Street>> GetStreetByRouteIdAsync(long routeId)
        {
            return await _streetRepository.GetStreetByRouteIdAsync(routeId);
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
        public async Task DeleteStreetAsync(Street street)
        {
            await _streetRepository.DeleteStreetAsync(street);
        }

        public async Task<string> GeometryToWkt(List<Coordinate> coordinates)
        {
            await Task.Delay(0);

            if (coordinates == null || coordinates.Count == 0)
            {
                return string.Empty;
            }

            // Construct the LINESTRING WKT string
            StringBuilder wktBuilder = new StringBuilder();

            if(coordinates.Count > 1)
                wktBuilder.Append("LINESTRING (");
            else if(coordinates.Count == 1)
                wktBuilder.Append("POINT (");

            foreach (Coordinate coord in coordinates)
            {
                wktBuilder.Append(coord.X).Append(" ").Append(coord.Y).Append(", ");
            }

            // Remove the trailing comma and space
            wktBuilder.Length -= 2;
            wktBuilder.Append(")");

            return wktBuilder.ToString();
        }

        public async Task<string> LocationToWkt(Coordinate coordinate)
        {
            await Task.Delay(0);

            // Construct the LINESTRING WKT string
            StringBuilder wktBuilder = new StringBuilder();
            
            wktBuilder.Append("POINT (");


            wktBuilder.Append(coordinate.X).Append(" ").Append(coordinate.Y).Append(", ");

            // Remove the trailing comma and space
            wktBuilder.Length -= 2;
            wktBuilder.Append(")");

            return wktBuilder.ToString();
        }
    }
}
