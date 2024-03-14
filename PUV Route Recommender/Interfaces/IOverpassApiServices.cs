using PUV_Route_Recommender.DTO;
using PUV_Route_Recommender.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Interfaces
{
    public interface IOverpassApiServices
    {
        Task<List<Route>> GetOSMData();
    }
}
