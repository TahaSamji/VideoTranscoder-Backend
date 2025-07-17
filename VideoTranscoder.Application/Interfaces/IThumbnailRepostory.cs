using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IThumbnailRepository
    {
        Task SaveAsync(Thumbnail data); // Save a single thumbnail to the database

        Task<Thumbnail?> GetByIdAsync(int id); // Get thumbnail by ID

        Task SaveAllAsync(List<Thumbnail> thumbnails); // Save multiple thumbnails in bulk

        Task<List<Thumbnail>> GetAllThumbnailsAsync(int fileId); // Get all thumbnails for a specific video file

        Task SetDefaultThumbnailAsync(int thumbnailId); // Set a thumbnail as the default for its video

        Task<int> CountThumbnailsByFileIdAsync(int fileId); // Count the number of thumbnails for a video
    }
}
