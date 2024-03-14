using PUV_Route_Recommender.DTO;
using PUV_Route_Recommender.Interfaces;
using PUV_Route_Recommender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Services
{
    public class OverpassApiServices : IOverpassApiServices
    {
        HttpClient _httpClient;
        public OverpassApiServices()
        {
            _httpClient = new HttpClient();
        }

        List<Route> routes = new();
        public async Task<List<Route>> GetOSMData()
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

            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            //parse json data to DTO
            var osmData = JsonSerializer.Deserialize<OSMDataDTO>(responseBody);


            //map osm relations to route model
            foreach (OSMRelationDTO relation in osmData.elements)
            {
                Route route = new Route()
                {
                    Osm_Id = relation.id,
                    Code = relation.tags.TryGetValue("ref", out string code) ? code : "No Code",
                    Name = relation.tags.TryGetValue("name", out string name) ? name : "No Name"
                };
                routes.Add(route);
            }
            return routes;
        }
    }
}
