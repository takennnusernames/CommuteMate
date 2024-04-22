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
        public async Task UpdateStreetAsync(Street street)
        {
            await _streetRepository.UpdateStreetAsync(street);
        }

        public async Task<string> StreetListToWkt(List<Coordinate> coordinates)
        {
            await Task.Delay(0);

            if (coordinates == null || coordinates.Count == 0)
            {
                return string.Empty;
            }

            // Construct the LINESTRING WKT string
            StringBuilder wktBuilder = new StringBuilder();
            wktBuilder.Append("LINESTRING (");

            foreach (Coordinate coord in coordinates)
            {
                wktBuilder.Append(coord.Y).Append(" ").Append(coord.X).Append(", ");
            }

            // Remove the trailing comma and space
            wktBuilder.Length -= 2;
            wktBuilder.Append(")");

            return wktBuilder.ToString();
        }

        public async Task<List<Coordinate>> WktToStreetList(string wktString)
        {
            await Task.Delay(0);

            if (string.IsNullOrWhiteSpace(wktString))
            {
                return new List<Coordinate>();
            }

            // Find the index of the opening and closing parentheses
            int startIndex = wktString.IndexOf('(');
            int endIndex = wktString.LastIndexOf(')');

            // Extract the substring between the parentheses
            string coordinatesString = wktString.Substring(startIndex + 1, endIndex - startIndex - 1);

            // Split coordinates string into individual coordinates
            string[] coordinateStrings = coordinatesString.Split(',');

            List<Coordinate> coordinates = new List<Coordinate>();

            foreach (var coordinateString in coordinateStrings)
            {
                // Split coordinate string into X and Y
                string[] parts = coordinateString.Trim().Split(' ');

                // Parse X and Y values
                if (parts.Length == 2 && double.TryParse(parts[0], out double x) && double.TryParse(parts[1], out double y))
                {
                    coordinates.Add(new Coordinate { X = x, Y = y });
                }
                else
                {
                    // Handle invalid coordinate format
                    throw new FormatException("Invalid coordinate format in WKT string.");
                }
            }

            return coordinates;
        }

        public async Task StreetSequenceOrder(List<StreetWithNode> streets)
        {
            await Task.Delay(0);
            List<StreetWithNode> sequencedStreet = [];

            sequencedStreet.Add(streets.First());
            while (sequencedStreet.Count != streets.Count)
            {
                //if last street is found???
                var lastStreet = sequencedStreet.Last();
                var lastNode = lastStreet.Nodes.Last();
                var nextStreet = streets.Find(s => s.Nodes.First() == lastNode);
                if (nextStreet != sequencedStreet.First())
                    sequencedStreet.Add(nextStreet);
                else if(sequencedStreet.Contains(nextStreet))
                {
                    // Get the index of the first item
                    int firstIndex = sequencedStreet.IndexOf(sequencedStreet.Find(s => s.Osm_Id == nextStreet.Osm_Id));

                    // Insert the next street before the first item
                    sequencedStreet.Insert(firstIndex, nextStreet);
                }
            }
            streets = sequencedStreet;
        }

    }
}
