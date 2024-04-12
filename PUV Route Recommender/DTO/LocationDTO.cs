using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.DTO
{
    public class Address
    {
        [JsonProperty("ISO3166-2-lvl4")]
        public string ISO31662lvl4 { get; set; }
        public string borough { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string country_code { get; set; }
        public string historic { get; set; }
        public string house_number { get; set; }
        public string neighbourhood { get; set; }
        public string postcode { get; set; }
        public string road { get; set; }
        public string suburb { get; set; }
    }

    public class LocationDTO
    {
        public Address address { get; set; }
        public List<string> boundingbox { get; set; }
        public string @class { get; set; }
        public string display_name { get; set; }
        public double importance { get; set; }
        public string lat { get; set; }
        public string licence { get; set; }
        public string lon { get; set; }
        public long osm_id { get; set; }
        public string osm_type { get; set; }
        public int place_id { get; set; }
        public string svg { get; set; }
        public string type { get; set; }
    }
}
