using PUV_Route_Recommender.DTO;
using PUV_Route_Recommender.Interfaces;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static Android.App.DownloadManager;

namespace PUV_Route_Recommender.Services
{
    public class RouteService : IRouteService
    {
        HttpClient _httpClient;

        public RouteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        List<Route> routes = new();
        public async Task<List<Route>> GetRoutesAsync()
        {
            if (routes?.Count > 0)
                return routes;

            string url = "https://overpass-api.de/api/interpreter";
            string query = @"
                [out:json]
;
                area(3612455830)->.searchArea; /*cebu city area*/
                relation
                  [""route""=""bus""]
                  (area.searchArea);
                out body;";
            var response = await _httpClient.GetAsync($"{url}?data={Uri.EscapeDataString(query)}");

            if (response.IsSuccessStatusCode)
            {
                var osmData = await response.Content.ReadFromJsonAsync<OSMDataDTO>();
                routes = osmData.elements.OfType<OSMRelationDTO>()
                         .Select(r => new Route
                         {
                             Osm_Id = r.id,
                             Code = r.tags.TryGetValue("ref", out string code) ? code : "No Code",
                             Name = r.tags.TryGetValue("name", out string name) ? name : "No Name"
                         })
                         .ToList();
            }
            return routes;
        } 
    }
}
