using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.DTO
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Address
    {
        public string amenity { get; set; }
        public string house_number { get; set; }
        public string road { get; set; }
        public string quarter { get; set; }
        public string city { get; set; }
        public string region { get; set; }

        [JsonProperty("ISO3166-2-lvl3")]
        public string ISO31662lvl3 { get; set; }
        public string postcode { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
    }

    public class LocationDTO
    {
        public long place_id { get; set; }
        public string licence { get; set; }
        public string osm_type { get; set; }
        public long osm_id { get; set; }
        public string lat { get; set; }
        public string lon { get; set; }
        public string category { get; set; }
        public string type { get; set; }
        public int place_rank { get; set; }
        public double importance { get; set; }
        public string addresstype { get; set; }
        public string name { get; set; }
        public string display_name { get; set; }
        public Address address { get; set; }
        public List<string> boundingbox { get; set; }
    }


}
