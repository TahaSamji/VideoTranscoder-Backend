

using Microsoft.EntityFrameworkCore;
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

        public async Task SaveAllAsync(List<Thumbnail> thumbnails)
        {
            _dbContext.Thumbnails.AddRange(thumbnails);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<Thumbnail>> GetAllThumbnailsAsync(int fileId)
        {
            return await _dbContext.Thumbnails
                .Where(t => t.FileId == fileId)
                .ToListAsync();
        }
        public async Task SetDefaultThumbnailAsync(int thumbnailId)
        {
            var thumbnail = await _dbContext.Thumbnails.FindAsync(thumbnailId);

            if (thumbnail == null)
                throw new Exception("Thumbnail not found.");

            var currentDefaultThumbnail = await _dbContext.Thumbnails
       .FirstOrDefaultAsync(t => t.FileId == thumbnail.FileId && t.IsDefault && t.Id != thumbnailId);

            if (currentDefaultThumbnail != null)
            {
                currentDefaultThumbnail.IsDefault = false;
                _dbContext.Thumbnails.Update(currentDefaultThumbnail);
            }


            // Set the selected one as default
            thumbnail.IsDefault = true;
            _dbContext.Thumbnails.Update(thumbnail);

            await _dbContext.SaveChangesAsync();
        }

        public async Task<int> CountThumbnailsByFileIdAsync(int fileId)
{
    return await _dbContext.Thumbnails
        .Where(t => t.FileId == fileId)
        .CountAsync();
}




    }
}
