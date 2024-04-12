using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUV_Route_Recommender.Interfaces
{
    public interface IStreetRepository
    {
        Task<int> InsertStreetAsync(Street street);
        Task<Street> GetStreetByWayIdAsync(long wayId);
        Task<int> GetStreetIdAsync(string name);
        Task<Street> GetStreetByIdAsync(int id);
    }
}
