using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface ITranscodingJobRepository
    {
        Task SaveAsync(TranscodingJob job);
        Task<TranscodingJob?> GetByFileAndProfileAsync(int fileId, int profileId);
        Task<TranscodingJob?> GetByIdAsync(int id);
        Task UpdateStatusAsync(int jobId, string newStatus);
    }

}