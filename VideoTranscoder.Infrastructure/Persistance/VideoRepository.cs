
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Persistance
{
    public class VideoRepository : IVideoRepository
    {
        private readonly AppDbContext _dbContext;

        public VideoRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveAsync(VideoMetaData file)
        {
            _dbContext.VideoMetaDatas.Add(file);
            await _dbContext.SaveChangesAsync();
        }
          public async Task<VideoMetaData?> GetByIdAsync(int id)
        {
            return await _dbContext.VideoMetaDatas.FindAsync(id);
        }

    }
}
