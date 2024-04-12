using NetTopologySuite.Geometries;
using PUV_Route_Recommender.DTO;
using PUV_Route_Recommender.Interfaces;
using System.Net.Http.Json;
using Linestring = NetTopologySuite.Geometries.LineString;


namespace PUV_Route_Recommender.Services
{
    public class OverpassApiServices : IOverpassApiServices
    {
        HttpClient _httpClient;
        readonly IStreetService _streetService;
        readonly IRouteService _routeService;
        readonly IMapServices _mapServices;
        readonly IRouteStreetService _routeStreetService;
        public OverpassApiServices(
            IStreetService streetService,
            IRouteService routeService,
            IMapServices mapServices,
            IRouteStreetService routeStreetService)
        {
            _httpClient = new HttpClient();
            _streetService = streetService;
            _routeService = routeService;
            _mapServices = mapServices;
            _routeStreetService = routeStreetService;
        }

        public async Task RetrieveOverpassRoutesAsync()
        {
            string url = "https://overpass-api.de/api/interpreter";
            string query = @"
                [out:json];
                area(3612455830)->.searchArea; /*cebu city area*/
                relation[""route""=""bus""](area.searchArea);
                out tags;";
            var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}");

            if (response.IsSuccessStatusCode)
            {
                var osmData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                foreach (var element in osmData.elements)
                {
                    var code = element.tags.TryGetValue("ref", out string routeCode) ? routeCode : "No Code";
                    if (code.Contains("Ceres"))
                        continue;
                    int routeId = await _routeService.InsertRouteAsync(new Route
                    {
                        Osm_Id = element.id,
                        Code = code,
                        Name = element.tags.TryGetValue("name", out string name) ? name : "No Name"
                    });
                }
            }
        }
        public async Task RetrieveOverpassRouteStreetsAsync(long OsmId, int routeId)
        {
            string url = "https://overpass-api.de/api/interpreter";
            string query = $@"
                [out:json];
                relation({OsmId});
                way(r);
                out geom;";
            var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}");
            if (response.IsSuccessStatusCode)
            {
                var osmData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                foreach(var element in osmData.elements)
                {
                    var points = new List<Coordinate>();
                    foreach (var coordinate in element.geometry)
                    {
                        points.Add(new Coordinate(coordinate.lon, coordinate.lat));
                    }
                    string linestringWKT = _mapServices.StreetListToWkt(points);
                    int streetId = await _streetService.InsertStreetAsync(new Street
                    {
                        Way_Id = element.id,
                        Name = element.tags.TryGetValue("name", out string name) ? name : "No Name",
                        GeometryWKT = linestringWKT
                    });
                    await _routeStreetService.AddRouteStreetAsync(routeId, streetId);

                }

            }
        }
    }
}
