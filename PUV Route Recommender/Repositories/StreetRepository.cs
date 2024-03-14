using Microsoft.Maui.Controls;
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
    public class StreetRepository
    {
        public List<Street> _streets = new List<Street>();
        private readonly RouteRepository _routeRepository;
        public StreetRepository(RouteRepository routeRepository)
        {
            _routeRepository = routeRepository;
        }
        public async Task<List<string>> GetStreets(int Osm_Id)
        {
            List<string> streetList = await _routeRepository.GetRouteStreets(Osm_Id);

            return streetList;
        }
    }
}
