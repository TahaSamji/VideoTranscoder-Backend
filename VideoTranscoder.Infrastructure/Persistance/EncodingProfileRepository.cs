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

        public async Task SaveAsync(EncodingProfile data)
        {
            // Add or update the EncodingProfile
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

        public async Task<EncodingProfile?> GetByIdAsync(int id)
        {
            return await _dbcontext.EncodingProfiles.FindAsync(id);
        }
        public async Task<List<EncodingProfile>> GetAllAsync(int pageNumber, int pageSize)
        {
            return await _dbcontext.EncodingProfiles
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<int> GetTotalCountAsync()
        {
            return await _dbcontext.EncodingProfiles.CountAsync();
        }

        public async Task<List<EncodingProfile>> GetProfilesUpToHeightAsync(int maxHeight)
        {
            return await _dbcontext.EncodingProfiles
                .Where(p => p.Height <= maxHeight)
                .OrderByDescending(p => p.Height)
                .ThenByDescending(p => p.Width)
                .ToListAsync();
        }
    }
}
