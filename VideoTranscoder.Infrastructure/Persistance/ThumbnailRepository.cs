

using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Persistance
{
    public class ThumbnailRepository : IThumbnailRepository
    {
        private readonly AppDbContext _dbContext;

        public ThumbnailRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveAsync(Thumbnail thumbnail)
        {
            _dbContext.Thumbnails.Add(thumbnail);
            await _dbContext.SaveChangesAsync();
        }
          public async Task<Thumbnail?> GetByIdAsync(int id)
        {
            return await _dbContext.Thumbnails.FindAsync(id);
        }

    }
}
