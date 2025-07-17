using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IVideoRepository
    {
        // Saves a new VideoMetaData entity to the database.
        Task SaveAsync(VideoMetaData data);

        // Retrieves a video by its unique ID.
        Task<VideoMetaData?> GetByIdAsync(int id);

        // Searches for a video using its name, size
        Task<VideoMetaData?> FindByNameAndSizeAsync(string name, long size, int userId);

        // Returns a paginated list of all videos uploaded by a specific user.
        Task<List<VideoMetaData>> GetAllByUserIdAsync(int userId, int page, int pageSize);

        // Updates the default thumbnail URL for a video.
        Task UpdateThumbnailUrlAsync(int videoId, string thumbnailUrl);

        // Updates the current status of a video (e.g., Uploaded, Transcoding, Completed).
        Task UpdateStatusAsync(int videoId, string newStatus);
    }
}
