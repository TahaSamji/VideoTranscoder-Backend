using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IVideoVariantRepository
    {
        // Persists a single video variant (e.g., a transcoded rendition) to the database.
        Task SaveAsync(VideoVariant variant);

        // Retrieves all video variants for a given file only if the transcoding job has completed.
        Task<List<VideoVariant>> GetVariantsByFileIdIfCompletedAsync(int fileId);
    }
}
