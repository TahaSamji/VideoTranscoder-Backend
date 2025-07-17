using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IVideoService
    {
        // Retrieves a paginated list of all videos for a specific user.
        Task<List<VideoMetaData>> GetAllVideosByUserIdAsync(int userId, int page, int pageSize);

        // Retrieves all transcoded renditions (HLS, DASH, etc.) for a given video file.
        Task<List<VideoRenditionDto>> GetVideoRenditionsByFileIdAsync(int fileId);

        // Stores video metadata and generates thumbnails after file merging is completed.
        Task StoreFileAndGenerateThumbnailsAsync(MergeRequestDto request, int userId);
    }
}
