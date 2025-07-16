

using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{


    public interface IVideoService
    {
        Task<List<VideoMetaData>> GetAllVideosByUserIdAsync(int userId, int page, int pageSize);
        Task<List<VideoRenditionDto>> GetVideoRenditionsByFileIdAsync(int fileId);
        Task StoreFileAndGenerateThumbnailsAsync(MergeRequestDto request, int userId);
    }
}