﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IRouteStreetRepository
    {
        Task<bool> InsertStreetRelation(RouteStreet routeStreet);
        Task<List<Street>> GetRelatedStreets(long osmId);
        bool CheckRelation(long streetId, long routeId);
    }
}
