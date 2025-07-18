using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface ITranscodingJobRepository
    {
        // Saves a new transcoding job record to the database
        Task SaveAsync(TranscodingJob job);

        // Retrieves a transcoding job by FileId and EncodingProfileId
        Task<TranscodingJob?> GetByFileAndProfileAsync(int fileId, int profileId);

        // Retrieves a transcoding job by its unique Id
        Task<TranscodingJob?> GetByIdAsync(int id);

        // Updates the status (e.g., Pending, Completed, Failed) of a job by its Id
        Task UpdateStatusAsync(int jobId, string newStatus);

        // Counts the number of Finished jobs for a specific video file
        Task<int> CountFinishedJobsByFileIdAsync(int fileId);

        // Updates the job status to error and saves the error message
        Task UpdateErrorStatusAsync(int id, string v, string message);
    }
}
