using Microsoft.EntityFrameworkCore;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Persistance
{
    public class TranscodingJobRepository : ITranscodingJobRepository
    {
        private readonly AppDbContext _dbContext;

        public TranscodingJobRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Saves a new transcoding job record to the database.
        /// </summary>
        public async Task SaveAsync(TranscodingJob job)
        {
            _dbContext.TranscodingJobs.Add(job);
            await _dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a transcoding job by its ID.
        /// </summary>
        public async Task<TranscodingJob?> GetByIdAsync(int id)
        {
            return await _dbContext.TranscodingJobs.FindAsync(id);
        }

        /// <summary>
        /// Retrieves a transcoding job by file ID and encoding profile ID.
        /// </summary>
        public async Task<TranscodingJob?> GetByFileAndProfileAsync(int fileId, int profileId)
        {
            return await _dbContext.TranscodingJobs
                .FirstOrDefaultAsync(j => j.VideoFileId == fileId && j.EncodingProfileId == profileId);
        }

        /// <summary>
        /// Updates the status of a transcoding job.
        /// </summary>
        public async Task UpdateStatusAsync(int jobId, string newStatus)
        {
            var job = await _dbContext.TranscodingJobs.FindAsync(jobId);
            if (job != null)
            {
                job.Status = newStatus;
                await _dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves the latest status of a transcoding job by file ID.
        /// </summary>
        public async Task<string?> GetStatusByFileIdAsync(int fileId)
        {
            var job = await _dbContext.TranscodingJobs
                .OrderByDescending(j => j.CreatedAt)
                .FirstOrDefaultAsync(j => j.VideoFileId == fileId);

            return job?.Status;
        }

        /// <summary>
        /// Counts the number of completed transcoding jobs for a given file ID.
        /// </summary>
        public async Task<int> CountCompletedJobsByFileIdAsync(int fileId)
        {
            return await _dbContext.TranscodingJobs
                .Where(j => j.VideoFileId == fileId && j.Status == "Completed")
                .CountAsync();
        }

        /// <summary>
        /// Updates the job status and error message in case of a transcoding failure.
        /// </summary>
        public async Task UpdateErrorStatusAsync(int jobId, string status, string? errorMessage = null)
        {
            var job = await _dbContext.TranscodingJobs.FirstOrDefaultAsync(j => j.Id == jobId);
            if (job == null)
            {
                throw new InvalidOperationException($"TranscodingJob with ID {jobId} not found.");
            }

            job.Status = status;
            job.ErrorMessage = errorMessage!;
            await _dbContext.SaveChangesAsync();
        }
    }
}
