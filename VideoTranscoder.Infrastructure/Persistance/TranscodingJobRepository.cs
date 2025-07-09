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

        public async Task SaveAsync(TranscodingJob job)
        {
            _dbContext.TranscodingJobs.Add(job);
            await _dbContext.SaveChangesAsync();
        }
        public async Task<TranscodingJob?> GetByIdAsync(int id)
        {
            return await _dbContext.TranscodingJobs.FindAsync(id);
        }
        
        public async Task<TranscodingJob?> GetByFileAndProfileAsync(int fileId, int profileId)
    {
        return await _dbContext.TranscodingJobs
            .FirstOrDefaultAsync(j => j.VideoFileId == fileId && j.EncodingProfileId == profileId);
    }


    }
}
