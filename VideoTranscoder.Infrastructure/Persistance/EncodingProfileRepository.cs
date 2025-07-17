using Microsoft.EntityFrameworkCore;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Persistance
{
    public class EncodingProfileRepository : IEncodingProfileRepository
    {
        private readonly AppDbContext _dbcontext;

        public EncodingProfileRepository(AppDbContext context)
        {
            _dbcontext = context;
        }

        /// <summary>
        /// Saves a new encoding profile or updates an existing one based on its ID.
        /// </summary>
        public async Task SaveAsync(EncodingProfile data)
        {
            var existing = await _dbcontext.EncodingProfiles.FindAsync(data.Id);
            if (existing == null)
            {
                await _dbcontext.EncodingProfiles.AddAsync(data);
            }
            else
            {
                _dbcontext.Entry(existing).CurrentValues.SetValues(data);
            }

            await _dbcontext.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _dbcontext.EncodingProfiles.FindAsync(id);
            if (existing == null)
                return false;

            existing.IsActive = false;
            await _dbcontext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves a single encoding profile by its ID.
        /// </summary>
        public async Task<EncodingProfile?> GetByIdAsync(int id)
        {
            return await _dbcontext.EncodingProfiles.FindAsync(id);
        }

        /// <summary>
        /// Retrieves a paginated list of encoding profiles, ordered by creation date (descending).
        /// </summary>
        public async Task<List<EncodingProfile>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _dbcontext.EncodingProfiles
                .Where(e => e.IsActive) //  Filter only active ones
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        /// <summary>
        /// Returns the total number of encoding profiles in the database.
        /// </summary>
        public async Task<int> GetTotalCountAsync()
        {
            return await _dbcontext.EncodingProfiles.CountAsync();
        }

        /// <summary>
        /// Retrieves all encoding profiles with a height less than or equal to the specified max height.
        /// Ordered by height (descending), then width (descending).
        /// </summary>
        public async Task<List<EncodingProfile>> GetProfilesUpToHeightAndBrowserTypeAsync(int maxHeight,string browserType)
        {
            return await _dbcontext.EncodingProfiles
                .Where(p => p.Height <= maxHeight && p.BrowserType == browserType && p.IsActive)
                .OrderByDescending(p => p.Height)
                .ThenByDescending(p => p.Width)
                .ToListAsync();
        }
    }
}
