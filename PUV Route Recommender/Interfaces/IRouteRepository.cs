﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IRouteRepository
    {
        Task<int> InsertRouteAsync(Route route);
        Task<IEnumerable<Route>> GetAllRoutesAsync();
        Task<Route> GetRouteByIdAsync(int id);
        Task<int> CountRoutesAsync();
        Task<Route> GetRouteByOsmIdAsync(long id);
        Task UpdateRouteAsync(Route route);
    }
}