using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IThumbnailRepository
    {
        Task SaveAsync(Thumbnail data);
        Task<Thumbnail?> GetByIdAsync(int id);
    }

}