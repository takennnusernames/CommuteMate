using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IStreetRepository
    {
        Task<Street> InsertStreetAsync(Street street);
        Task<Street> GetStreetByOsmIdAsync(long wayId);
        Task<int> GetStreetIdAsync(string name);
        Task<Street> GetStreetByIdAsync(int id);
    }
}
