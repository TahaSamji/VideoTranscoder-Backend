

using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{

    public interface IThumbnailService
    {
        Task GenerateAndStoreThumbnailsAsync(string fileName, int userId, int fileId);
        Task<List<ThumbnailDto>> GetAllThumbnailsAsync(int fileId);
        Task SetDefaultThumbnailAsync(int thumbnailId, int fileId);
         Task<int> CountThumbnailsForFileAsync(int fileId);
    }
}