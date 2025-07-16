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

        /// <summary>
        /// Saves a single thumbnail entity to the database.
        /// </summary>
        public async Task SaveAsync(Thumbnail thumbnail)
        {
            _dbContext.Thumbnails.Add(thumbnail);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a thumbnail by its ID.
        /// </summary>
        public async Task<Thumbnail?> GetByIdAsync(int id)
        {
            return await _dbContext.Thumbnails.FindAsync(id);
        }

        /// <summary>
        /// Saves a list of thumbnails to the database in a batch.
        /// </summary>
        public async Task SaveAllAsync(List<Thumbnail> thumbnails)
        {
            _dbContext.Thumbnails.AddRange(thumbnails);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves all thumbnails associated with a specific video file ID.
        /// </summary>
        public async Task<List<Thumbnail>> GetAllThumbnailsAsync(int fileId)
        {
            return await _dbContext.Thumbnails
                .Where(t => t.FileId == fileId)
                .ToListAsync();
        }

        /// <summary>
        /// Sets the specified thumbnail as the default for its associated video file.
        /// </summary>
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

            thumbnail.IsDefault = true;
            _dbContext.Thumbnails.Update(thumbnail);

            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Returns the count of thumbnails associated with a specific video file ID.
        /// </summary>
        public async Task<int> CountThumbnailsByFileIdAsync(int fileId)
        {
            return await _dbContext.Thumbnails
                .Where(t => t.FileId == fileId)
                .CountAsync();
        }
    }
}
