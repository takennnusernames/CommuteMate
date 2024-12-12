using CommuteMate.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CommuteMate.Repositories
{
    public class DownloadsRepository : IDownloadsRepository
    {
        private readonly CommuteMateDbContext _dbContext;
        public DownloadsRepository(CommuteMateDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OfflineStep> createStep(OfflineStep step)
        {
            try
            {
                await _dbContext.AddAsync(step);
                await _dbContext.SaveChangesAsync();
                return step;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create step: {ex.Message}");
                throw;
            }
        }

        public async Task<PathStep> createPathStep(PathStep pathStep)
        {
            try 
            {
                await _dbContext.AddAsync(pathStep);
                await _dbContext.SaveChangesAsync();
                return pathStep;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to insert relation: {ex.Message}");
                throw;
            }
        }

        public async Task<Summary> createSummary(Summary summary)
        {
            try
            {
                await _dbContext.AddAsync(summary);
                await _dbContext.SaveChangesAsync();
                return summary;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create summary: {ex.Message}");
                throw;
            }
        }

        public async Task<OfflinePath> createPath(OfflinePath path)
        {
            try
            {
                await _dbContext.AddAsync(path);
                await _dbContext.SaveChangesAsync();
                return path;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create path: {ex.Message}");
                throw;
            }
        }

        public async Task updatePath(OfflinePath path)
        {
            try
            {
                _dbContext.Entry(path).State = EntityState.Modified;
                await _dbContext.SaveChangesAsync();
                return;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Failed to update path: {ex.Message}");
                throw;
            }
        }
        public async Task<List<OfflinePath>> GetAllOfflinePathsAsync()
        {
            try
            {
                var paths = _dbContext.OfflinePaths
                    .Include(op => op.Summary)
                    .Include(op => op.PathSteps)
                    .ThenInclude(ps => ps.Step)
                    .ToList();
                if (paths is not null)
                    return await Task.FromResult(paths);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrive paths: {ex.Message}");
                throw;
            }
        }
        public async Task<List<OfflineStep>> GetPathStep(int id)
        {
            try
            {
                var steps = await _dbContext.PathSteps
                            .Where(ps => ps.PathId == id)
                            .Include(ps => ps.Step) // Include Step entity through PathStep
                            .Select(ps => ps.Step)  // Select only Step entities
                            .ToListAsync();
                if (steps is not null)
                    return steps;
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retrive paths: {ex.Message}");
                throw;
            }
        }
        public async Task<OfflinePath> GetOfflinePathByIdAsync(int id)
        {
            var path = await _dbContext.OfflinePaths.FindAsync(id);
            if (path == default)
                return null;
            return path;
        }

        public async Task DeleteOfflinePathAsync(OfflinePath path)
        {
            try
            {
                _dbContext.OfflinePaths.Remove(path);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete path: {ex.Message}");
                throw;
            }
        }
    }
}
