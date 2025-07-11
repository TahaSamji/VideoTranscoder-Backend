// VideoVariantRepository.cs
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
}