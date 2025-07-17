using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IThumbnailService
    {
        Task<string> GenerateAndStoreThumbnailsAsync(string fileName, int userId, int fileId, string filePath); 
        // Generates thumbnails from the video file and stores them in cloud/local storage

        Task<List<ThumbnailDto>> GetAllThumbnailsAsync(int fileId); 
        // Retrieves all thumbnails associated with the given video file

        Task SetDefaultThumbnailAsync(int thumbnailId, int fileId); 
        // Sets a specific thumbnail as the default for the given video

        Task<int> CountThumbnailsForFileAsync(int fileId); 
        // Returns the total count of thumbnails for a specific video file
    }
}
