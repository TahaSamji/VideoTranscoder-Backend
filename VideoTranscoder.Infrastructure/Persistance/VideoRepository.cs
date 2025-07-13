
using Microsoft.EntityFrameworkCore;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Persistance
{
    public class VideoRepository : IVideoRepository
    {
        private readonly AppDbContext _dbContext;

        public VideoRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveAsync(VideoMetaData file)
        {
            _dbContext.VideoMetaDatas.Add(file);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<VideoMetaData?> GetByIdAsync(int id)
        {
            return await _dbContext.VideoMetaDatas.FindAsync(id);
        }

        public async Task<VideoMetaData?> FindByNameAndSizeAsync(string name, long size, int userId)
        {
            return await _dbContext.VideoMetaDatas
                .Where(v => v.OriginalFilename == name && v.Size == size && v.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<VideoMetaData>> GetAllByUserIdAsync(int userId, int page, int pageSize)
        {
            return await _dbContext.VideoMetaDatas
       .Where(v => v.UserId == userId)
       .OrderByDescending(v => v.CreatedAt)
       .Skip((page - 1) * pageSize)
       .Take(pageSize)
       .ToListAsync();
        }

        public async Task UpdateThumbnailUrlAsync(int videoId, string thumbnailUrl)
        {
            var video = await _dbContext.VideoMetaDatas.FindAsync(videoId);
            if (video != null)
            {
                video.defaultThumbnailUrl = thumbnailUrl;
                video.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task UpdateStatusAsync(int videoId, string newStatus)
        {
            var video = await _dbContext.VideoMetaDatas.FindAsync(videoId);
            if (video != null)
            {
                video.Status = newStatus;
                video.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
