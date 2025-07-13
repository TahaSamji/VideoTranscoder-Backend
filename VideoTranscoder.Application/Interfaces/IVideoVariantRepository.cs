using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IVideoVariantRepository
    {
        Task SaveAsync(VideoVariant variant);
        Task<List<VideoVariant>> GetVariantsByFileIdIfCompletedAsync(int fileId);
    }
}