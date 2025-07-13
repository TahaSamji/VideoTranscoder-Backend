using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IThumbnailRepository
    {
        Task SaveAsync(Thumbnail data);
        Task<Thumbnail?> GetByIdAsync(int id);
        Task SaveAllAsync(List<Thumbnail> thumbnails);

        Task<List<Thumbnail>> GetAllThumbnailsAsync(int fileId);

        Task SetDefaultThumbnailAsync(int thumbnailId);

        Task<int> CountThumbnailsByFileIdAsync(int fileId);

    }
    

}