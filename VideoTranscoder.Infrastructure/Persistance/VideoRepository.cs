using Microsoft.EntityFrameworkCore;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Persistance
{
    // Implementation of the IVideoRepository interface for accessing and managing video metadata
    public class VideoRepository : IVideoRepository
    {
        private readonly AppDbContext _dbContext;

        // Constructor that injects the database context
        public VideoRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Saves a new video metadata record to the database
        public async Task SaveAsync(VideoMetaData file)
        {
            _dbContext.VideoMetaDatas.Add(file);         // Add video to the DbSet
            await _dbContext.SaveChangesAsync();         // Persist changes to DB
        }

        // Retrieves a video metadata entry by its unique ID
        public async Task<VideoMetaData?> GetByIdAsync(int id)
        {
            return await _dbContext.VideoMetaDatas.FindAsync(id); // Uses primary key lookup
        }

        // Finds a video by its name, size, and user ID (used to prevent duplicates)
        public async Task<VideoMetaData?> FindByNameAndSizeAsync(string name, long size, int userId)
        {
            return await _dbContext.VideoMetaDatas
                .Where(v => v.OriginalFilename == name && v.Size == size && v.UserId == userId)
                .FirstOrDefaultAsync(); // Returns the first matching video or null
        }

        // Retrieves paginated list of videos for a given user ID
        public async Task<List<VideoMetaData>> GetAllByUserIdAsync(int userId, int page, int pageSize)
        {
            return await _dbContext.VideoMetaDatas
                .Where(v => v.UserId == userId)                          // Filter by user
                .OrderByDescending(v => v.CreatedAt)                    // Sort by newest first
                .Skip((page - 1) * pageSize)                            // Pagination: skip records
                .Take(pageSize)                                         // Pagination: take page size
                .ToListAsync();                                         // Execute query and return results
        }

        // Updates the default thumbnail URL of a video
        public async Task UpdateThumbnailUrlAsync(int videoId, string thumbnailUrl)
        {
            var video = await _dbContext.VideoMetaDatas.FindAsync(videoId); // Fetch the video by ID
            if (video != null)
            {
                video.defaultThumbnailUrl = thumbnailUrl;           // Update the thumbnail URL
                video.UpdatedAt = DateTime.UtcNow;                  // Update modification timestamp
                await _dbContext.SaveChangesAsync();                // Save changes
            }
        }

        // Updates the status of a video (e.g., Processing, Completed, Failed)
        public async Task UpdateStatusAsync(int videoId, string newStatus)
        {
            var video = await _dbContext.VideoMetaDatas.FindAsync(videoId); // Find the video
            if (video != null)
            {
                video.Status = newStatus;                          // Set the new status
                video.UpdatedAt = DateTime.UtcNow;                // Update timestamp
                await _dbContext.SaveChangesAsync();              // Save changes to DB
            }
        }
    }
}
