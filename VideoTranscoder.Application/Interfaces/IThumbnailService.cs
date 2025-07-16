

using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{

    public interface IThumbnailService
    {
        Task<string> GenerateAndStoreThumbnailsAsync(string fileName, int userId, int fileId, string filePath);
        Task<List<ThumbnailDto>> GetAllThumbnailsAsync(int fileId);
        Task SetDefaultThumbnailAsync(int thumbnailId, int fileId);
        Task<int> CountThumbnailsForFileAsync(int fileId);
    }
}