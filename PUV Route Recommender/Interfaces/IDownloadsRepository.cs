using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommuteMate.Interfaces
{
    public interface IDownloadsRepository
    {
        Task<OfflineStep> createStep(OfflineStep step);
        Task<PathStep> createPathStep(PathStep pathStep);
        Task<Summary> createSummary(Summary summary);
        Task<OfflinePath> createPath(OfflinePath path);
        Task updatePath(OfflinePath path);
        Task<List<OfflinePath>> GetAllOfflinePathsAsync();
        Task<List<OfflineStep>> GetPathStep(int id);
        Task<OfflinePath> GetOfflinePathByIdAsync(int id);
        Task DeleteOfflinePathAsync(OfflinePath path);
    }
}
