using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IVideoRepository
    {
        Task SaveAsync(VideoMetaData data);
        Task<VideoMetaData?> GetByIdAsync(int id);
    }

}