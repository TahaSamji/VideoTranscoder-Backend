using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IVideoRepository
    {
        Task SaveAsync(VideoMetaData data);
        Task<VideoMetaData?> GetByIdAsync(int id);
        Task<VideoMetaData?> FindByNameAndSizeAsync(string name, long size, int userId);
        Task<List<VideoMetaData>> GetAllByUserIdAsync(int userId, int page, int pageSize);
        Task UpdateThumbnailUrlAsync(int videoId, string thumbnailUrl);
        Task  UpdateStatusAsync(int videoId, string newStatus);
        
    }

}