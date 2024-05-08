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

        public async Task<List<StreetWithNode>> StreetSequenceOrder(List<StreetWithNode> streets)
        {
            await Task.Delay(0);
            LinkedList<StreetWithNode> linkedSequencedStreet = new LinkedList<StreetWithNode>(streets);
            var currentNode = linkedSequencedStreet.First;
            while (currentNode != null)
            {
                StreetWithNode currentStreet = currentNode.Value;

                var lastNode = currentStreet.Nodes.Last();
                var nextStreet = streets.Find(s => s.Nodes.First() == lastNode);
                if (nextStreet == null)
                {
                    if(currentNode.Next != null)
                    {
                        if (currentNode.Previous == null)
                        {
                            linkedSequencedStreet.AddLast(currentNode);
                            currentNode = currentNode.Next;
                            continue;
                        }
                        var firstNode = currentStreet.Nodes.First();
                        var previousStreet = streets.Find(s => s.Nodes.Last() == firstNode);
                        var previousNode = currentNode.Previous;
                        while (previousStreet == previousNode.Value)
                        {
                            currentStreet = previousNode.Value;
                            firstNode = currentStreet.Nodes.First();
                            previousStreet = streets.Find(s => s.Nodes.Last() == firstNode);
                            previousNode = currentNode.Previous;
                        }
                        linkedSequencedStreet.AddAfter(linkedSequencedStreet.Find(previousStreet), currentStreet);

                    }
                }
                else if(currentNode.Next.Value != nextStreet)
                {
                    linkedSequencedStreet.Remove(linkedSequencedStreet.Find(nextStreet));
                    linkedSequencedStreet.AddAfter(currentNode, nextStreet);
                }

                currentNode = currentNode.Next;
            }
            return linkedSequencedStreet.ToList();
        }

        public async Task<LinkedList<StreetWithCoordinates>> StartToEndStreets(long startId, long endId, List<StreetWithCoordinates> streets, long routeId)
        {
            try
            {
                await Task.Yield();
                LinkedList<StreetWithCoordinates> streetSequence = [];
                streetSequence.AddFirst(streets.Find(s => s.Osm_Id == startId));
                streetSequence.AddLast(streets.Find(s => s.Osm_Id == endId));
                var streetNode = streetSequence.First;

                while (streetNode != streetSequence.Last)
                {
                    StreetWithCoordinates street = streetNode.Value;
                    List<StreetWithCoordinates> nextStreets = [];
                    Coordinate lastNode = new();
                    if (street.Role == "forward" || street.Role == "")
                        lastNode = street.Coordinates.Last();
                    else if (street.Role == "backward")
                        lastNode = street.Coordinates.First();

                    nextStreets = streets.Where(s => s.Coordinates.First().Equals(lastNode) && s.Osm_Id != street.Osm_Id).ToList();
                    if (nextStreets.Count == 0)
                    {
                        nextStreets = streets.Where(s => s.Coordinates.Last().Equals(lastNode) && s.Osm_Id != street.Osm_Id).ToList();
                        if(nextStreets.Count == 0)
                        {
                            var nextStreetIds = await CheckNeighboringStreets(routeId, lastNode);
                            var ids = nextStreetIds.Except(streetSequence.Select(s => s.Osm_Id)).Where(s => s != street.Osm_Id).ToList();
                            nextStreets = streets.Where(s => ids.Contains(s.Osm_Id)).ToList();
                        }
                    }

                    if(nextStreets.Count == 1)
                        streetSequence.AddAfter(streetNode,nextStreets[0]);
                    else
                    {
                        List< LinkedList < StreetWithCoordinates >> tempSequence = [];
                        foreach(var nextStreet in nextStreets)
                        {
                            var nextStreetSequence = await StartToEndStreets(nextStreet.Osm_Id, endId, streets, routeId);
                            tempSequence.Add(nextStreetSequence);
                        }
                        LinkedList<StreetWithCoordinates> minCountLinkedList = tempSequence
                                .OrderBy(list => list.Count) // Order by count
                                .FirstOrDefault(); // Select the first (i.e., with the lowest count) or null if the list is empty

                        LinkedListNode<StreetWithCoordinates> minCountFirstNode = minCountLinkedList?.First;
                        if (minCountFirstNode != null)
                        {
                            // Add minCountFirstNode after streetNode
                            streetSequence.AddAfter(streetNode, minCountFirstNode);

                            // Update the connections to make the Next nodes of minCountFirstNode follow
                            streetNode = minCountFirstNode; // Update streetNode to point to the newly added node

                            // Iterate through the Next nodes of minCountFirstNode and add them after streetNode
                            LinkedListNode<StreetWithCoordinates> current = minCountFirstNode;
                            while (current.Next != null)
                            {
                                LinkedListNode<StreetWithCoordinates> nextNode = current.Next;
                                streetSequence.AddAfter(streetNode, nextNode);
                                streetNode = nextNode;
                                current = nextNode;
                            }
                        }

                    }
                    streetNode = streetNode.Next; // Move the pointer

                    if (streetNode == null && streetNode != streetSequence.Last)
                    {
                        streetSequence.Clear(); // Clear the list if pointer is null and it's not the last node
                        return new LinkedList<StreetWithCoordinates>(); // Return an empty list
                    }
                }

                return streetSequence;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Failed", ex.Message);
                throw;
            }


        }


        public async Task<List<long>> CheckNeighboringStreets(long relationId, Coordinate coordinates)
        {
            try
            {
                await _cancellationTokenSource.CancelAsync();
                _cancellationTokenSource = new CancellationTokenSource();
                string url = "https://overpass-api.de/api/interpreter";
                string query = $@"
                [out:json];
                  relation({relationId});
                  way(around:100.00,{coordinates.Y}, {coordinates.X})(r);
                  out ids;";
                var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}", _cancellationTokenSource.Token);

                List<long> neighboringStreetIds = [];
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                    foreach (var element in responseData.elements)
                    {
                        neighboringStreetIds.Add(element.id);
                    }
                }

                return neighboringStreetIds;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed To Check Neighboring Streets", ex.Message);
                throw;
            }
        }

    }
}
