// VideoVariantRepository.cs
using Microsoft.EntityFrameworkCore;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

public class VideoVariantRepository : IVideoVariantRepository
{
    private readonly AppDbContext _dbContext;

    public VideoVariantRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    public async Task SaveAsync(VideoVariant variant)
    {
        _dbContext.VideoVariants.Add(variant);
        await _dbContext.SaveChangesAsync();
    }
    public async Task<List<VideoVariant>> GetVariantsByFileIdIfCompletedAsync(int fileId)
    {
        return await _dbContext.VideoVariants
            .Include(v => v.TranscodingJob)
            .Where(v => v.TranscodingJob.VideoFileId == fileId &&
                        v.TranscodingJob.Status == "Completed")
            .ToListAsync();
    }

}