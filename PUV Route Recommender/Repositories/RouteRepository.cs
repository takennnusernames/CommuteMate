using Microsoft.VisualBasic;
using PUV_Route_Recommender.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Repositories
{
    public class RouteRepository
    {
        private readonly HttpClient _httpClient;
        private List<Route> _routes;
        private List<string> _streets;
        public RouteRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
            //Create list for routes;
            _routes = new List<Route>();
            _streets = new List<string>();
        }

        public async Task<List<Route>> GetOsmRoutes()
        {
            //Overpass QL query
            string query = @"
                [out:json]
;
                area(3612455830)->.searchArea; /*cebu city area*/
                relation
                  [""route""=""bus""]
                  (area.searchArea);
                out tags;";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://overpass-api.de/api/interpreter"),
                Content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Get the JSON data from the response as a string
                string jsonData = await response.Content.ReadAsStringAsync();

                // Parse the JSON data using the JsonDocument class
                JsonDocument document = JsonDocument.Parse(jsonData);

                // Get the array of elements from the parsed JSON data
                JsonElement elements = document.RootElement.GetProperty("elements");


                // Iterate through the array of elements
                foreach (JsonElement element in elements.EnumerateArray())
                {
                    // Check if the element is a relation
                    if (element.GetProperty("type").GetString() == "relation")
                    {
                        // Get the tags object
                        JsonElement tagsProperty = element.GetProperty("tags");
                        //element.TryGetProperty("relation", out osmElement);

                        _routes.Add(new Route
                        {
                            Osm_Id = element.GetProperty("id").GetInt32(),
                            Code = tagsProperty.TryGetProperty("ref", out JsonElement refElement) ? refElement.GetString() : "No Code",
                            Name = tagsProperty.TryGetProperty("name", out JsonElement nameElement) ? nameElement.GetString() : "No Name",
                        });
                    }
                }
                return _routes;
            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
                return new List<Route>();
            }
        }
        public async Task<List<String>> GetRouteStreets(int Osm_Id)
        {
            //Overpass QL query
            string query = @"
                [out:json]
                relation(" + Osm_Id + @");
                way(r);
                out tags;";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://overpass-api.de/api/interpreter"),
                Content = new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded")
            };
            try
            {
                HttpResponseMessage response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Get the JSON data from the response as a string
                string jsonData = await response.Content.ReadAsStringAsync();

                // Parse the JSON data using the JsonDocument class
                JsonDocument document = JsonDocument.Parse(jsonData);

                // Get the array of elements from the parsed JSON data
                JsonElement tags = document.RootElement.GetProperty("elements").GetProperty("tags");

                // Iterate through the array of members
                foreach (JsonElement tag in tags.EnumerateArray())
                {
                    _streets.Add(tag.GetProperty("name").GetString());
                }
                return _streets;

            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("\nException Caught!");
                Debug.WriteLine("Message :{0} ", e.Message);
                return new List<string>();

            }
        }
        public Route GetRouteById(int route_Id)
        {
            return _routes.FirstOrDefault(x => x.Osm_Id == route_Id);
        }
    }
    
}
