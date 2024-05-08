using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Linemerge;
using CommuteMate.Interfaces;
using QuickGraph;
using QuickGraph.Algorithms.ShortestPath;
using BruTile.Wms;
using NetTopologySuite.Triangulate.QuadEdge;
using QuickGraph.Algorithms.Observers;

namespace CommuteMate.Services
{
    public class RouteService : IRouteService
    {
        IRouteRepository _routeRepository;
        private readonly IStreetService _streetService;

        public RouteService(IRouteRepository routeRepository, IStreetService streetService)
        {
            _routeRepository = routeRepository;
            _streetService = streetService;
        }
        public async Task<Route> InsertRouteAsync(Route route)
        {
            return await _routeRepository.InsertRouteAsync(route);
        }
        public async Task<List<Route>> GetAllRoutesAsync()
        {
            var routes = await _routeRepository.GetAllRoutesAsync();
            return routes.ToList();
        }
        public async Task<List<Street>> GetRouteStreets(int id)
        {
            var streets = await _routeRepository.GetRouteStreets(id);
            return streets.ToList();
        }

        public async Task<Route> GetRouteByIdAsync(int id)
        {
            return await _routeRepository.GetRouteByIdAsync(id);
        }
        public async Task<Route> GetRouteByOsmIdAsync(long id)
        {
            return await _routeRepository.GetRouteByOsmIdAsync(id);
        }
        public async Task UpdateRouteAsync(Route route)
        {
            await _routeRepository.UpdateRouteAsync(route);
        }
        public async Task<int> CountRoutesAsync()
        {
            return await _routeRepository.CountRoutesAsync();
        }
        public async Task<UndirectedGraph<Coordinate, Edge<Coordinate>>> StreetToGraph(List<StreetWithCoordinates> streets, long routeId)
        {
            var graph = new UndirectedGraph<Coordinate, Edge<Coordinate>>();
            List<StreetWithCoordinates> streetSequence = [];
            foreach (var street in streets)
            {
                if(street.Role == "backward")
                {
                    for (int i = street.Coordinates.Count - 1; i > 0; i--)
                    {
                        Coordinate start = street.Coordinates[i];
                        Coordinate end = street.Coordinates[i - 1];
                        // Check if vertices already exist in the graph
                        if (!graph.ContainsVertex(start))
                        {
                            graph.AddVertex(start);
                        }
                        if (!graph.ContainsVertex(end))
                        {
                            graph.AddVertex(end);
                        }
                                                                if(!graph.ContainsEdge(new Edge<Coordinate>(start, end)))
                                            // Add edge between the vertices
                                            graph.AddEdge(new Edge<Coordinate>(start, end));

                        if (end == street.Coordinates.Last())
                        {
                            streetSequence.Add(street);
                            var nextStreets = streets.Where(s => s.Coordinates.First().Equals(end) && s.Osm_Id != street.Osm_Id).ToList();
                            if (nextStreets.Count == 0)
                            {
                                nextStreets = streets.Where(s => s.Coordinates.Last().Equals(end) && s.Osm_Id != street.Osm_Id).ToList();
                                if (nextStreets.Count == 0)
                                {
                                    var nextStreetIds = await _streetService.CheckNeighboringStreets(routeId, end);
                                    var ids = nextStreetIds.Where(s => s != street.Osm_Id).ToList();
                                    nextStreets = streets.Where(s => ids.Contains(s.Osm_Id)).ToList();
                                    if (nextStreets == null || nextStreets.Count == 0)
                                        continue;
                                    foreach (var nextStreet in nextStreets)
                                    {
                                        if (nextStreet.Role == "backward")
                                        {
                                            start = street.Coordinates.Last();
                                            end = nextStreet.Coordinates.Last();

                                            if (streets.Any(s => s.Coordinates.First().Equals(end) && s.Osm_Id != street.Osm_Id))
                                                continue;
                                        }
                                        else
                                        {
                                            start = street.Coordinates.Last();
                                            end = nextStreet.Coordinates.First();

                                            if (streets.Any(s => s.Coordinates.Last().Equals(end) && s.Osm_Id != street.Osm_Id))
                                                continue;
                                        }
                                        // Check if vertices already exist in the graph
                                        if (!graph.ContainsVertex(start))
                                        {
                                            graph.AddVertex(start);
                                        }
                                        if (!graph.ContainsVertex(end))
                                        {
                                            graph.AddVertex(end);
                                        }
                                        if (!graph.ContainsEdge(new Edge<Coordinate>(start, end)))
                                            // Add edge between the vertices
                                            graph.AddEdge(new Edge<Coordinate>(start, end));
                                    }

                                }
                            }
                        }
                    }
                }
                else
                    for (int i = 0; i < street.Coordinates.Count - 1; i++)
                {
                    Coordinate start = street.Coordinates[i];
                    Coordinate end = street.Coordinates[i + 1];
                    // Check if vertices already exist in the graph
                    if (!graph.ContainsVertex(start))
                    {
                        graph.AddVertex(start);
                    }
                    if (!graph.ContainsVertex(end))
                    {
                        graph.AddVertex(end);
                    }
                        if (!graph.ContainsEdge(new Edge<Coordinate>(start, end)))
                            // Add edge between the vertices
                            graph.AddEdge(new Edge<Coordinate>(start, end));


                        if (end == street.Coordinates.Last())
                        {
                            streetSequence.Add(street);
                            var nextStreets = streets.Where(s => s.Coordinates.First().Equals(end) && s.Osm_Id != street.Osm_Id).ToList();
                            if (nextStreets.Count == 0)
                            {
                                nextStreets = streets.Where(s => s.Coordinates.Last().Equals(end) && s.Osm_Id != street.Osm_Id).ToList();
                                if (nextStreets.Count == 0)
                                {
                                    var nextStreetIds = await _streetService.CheckNeighboringStreets(routeId, end);
                                    var ids = nextStreetIds.Where(s => s != street.Osm_Id).ToList();
                                    nextStreets = streets.Where(s => ids.Contains(s.Osm_Id)).ToList();
                                    if (nextStreets == null || nextStreets.Count == 0)
                                        continue;
                                    foreach (var nextStreet in nextStreets)
                                    {
                                        if (nextStreet.Role == "backward")
                                        {
                                            start = street.Coordinates.Last();
                                            end = nextStreet.Coordinates.Last();

                                            if (streets.Any(s => s.Coordinates.First().Equals(end) && s.Osm_Id != street.Osm_Id))
                                                continue;
                                        }
                                        else
                                        {
                                            start = street.Coordinates.Last();
                                            end = nextStreet.Coordinates.First();

                                            if (streets.Any(s => s.Coordinates.Last().Equals(end) && s.Osm_Id != street.Osm_Id))
                                                continue;
                                        }
                                        // Check if vertices already exist in the graph
                                        if (!graph.ContainsVertex(start))
                                        {
                                            graph.AddVertex(start);
                                        }
                                        if (!graph.ContainsVertex(end))
                                        {
                                            graph.AddVertex(end);
                                        }
                                        if(!graph.ContainsEdge(new Edge<Coordinate>(start, end)))
                                            // Add edge between the vertices
                                            graph.AddEdge(new Edge<Coordinate>(start, end));
                                    }

                                }
                            }
                        }
                    }
            }
            return graph;

        }

        public UndirectedGraph<Coordinate, Edge<Coordinate>> StreetToGraph(List<Street> streets)
        {
            var graph = new UndirectedGraph<Coordinate, Edge<Coordinate>>();

            foreach (var street in streets)
            {
                for (int i = 0; i < street.Coordinates.Count - 1; i++)
                {
                    var start = street.Coordinates[i];
                    var end = street.Coordinates[i + 1];
                    if (!graph.ContainsVertex(start))
                        graph.AddVertex(start);
                    if (!graph.ContainsVertex(end))
                        graph.AddVertex(end);
                    if (!graph.ContainsEdge(new Edge<Coordinate>(start, end)))
                        graph.AddEdge(new Edge<Coordinate>(start, end));        
                }
            }
            return graph;

        }

        public IEnumerable<Edge<Coordinate>> GetShortetstPath(UndirectedGraph<Coordinate, Edge<Coordinate>> graph, Coordinate origin, Coordinate destination)
        {
            // Find the shortest path between vertices 0 and 5
            //Coordinate origin = new Coordinate(123.8953882, 10.3102911);
            //Coordinate destination = new Coordinate(123.9000549, 10.3039991);

            // Create a Dijkstra algorithm instance
            var dijkstra = new UndirectedDijkstraShortestPathAlgorithm<Coordinate, Edge<Coordinate>>(graph, edge => 1);

            // Attach a vertex cost end the algorithm
            dijkstra.SetRootVertex(origin);
            var vis = new UndirectedVertexPredecessorRecorderObserver<Coordinate, Edge<Coordinate>>();
            using (vis.Attach(dijkstra))
            {
                dijkstra.Compute();
            }

            // Retrieve the shortest path start the algorithm's state
            vis.TryGetPath(destination, out IEnumerable<Edge<Coordinate>> shortestPath);

            // Print the shortest path
            if (shortestPath != null)
            {
                return shortestPath;
            }
            else
            {
                Console.WriteLine("No path found.");
                throw new System.Exception("Error: no path");
            }
        }
    }
}
