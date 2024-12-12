﻿using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IStreetService
    {
        Task<Street> InsertStreetAsync(Street street);
        Task<Street> GetStreetByWayIdAsync(long wayId);
        Task<List<Street>> GetStreetByRouteIdAsync(long routeId);
        Task<int> GetStreetIdAsync(string name);
        Task<Street> GetStreetByIdAsync(int id);
        Task UpdateStreetAsync(Street street);
        Task DeleteStreetAsync(Street street);
        Task<string> GeometryToWkt(List<Coordinate> coordinates);
        Task<string> LocationToWkt(Coordinate coordinate);
    }
}
