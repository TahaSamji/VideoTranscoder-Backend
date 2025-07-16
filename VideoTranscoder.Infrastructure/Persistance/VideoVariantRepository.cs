// VideoVariantRepository.cs
using Microsoft.EntityFrameworkCore;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

/// <summary>
/// Repository class responsible for handling persistence and retrieval of video variant data.
/// </summary>
public class VideoVariantRepository : IVideoVariantRepository
{
    private readonly AppDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the repository with the given database context.
    /// </summary>
    /// <param name="dbContext">The application database context.</param>
    public VideoVariantRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Saves a new video variant entity to the database.
    /// </summary>
    /// <param name="variant">The video variant entity to be saved.</param>
    public async Task SaveAsync(VideoVariant variant)
    {
        _dbContext.VideoVariants.Add(variant); // Add the variant to the context
        await _dbContext.SaveChangesAsync();   // Persist changes to the database
    }

    /// <summary>
    /// Retrieves all video variants associated with a given video file ID.
    /// </summary>
    /// <param name="fileId">The ID of the video file to fetch variants for.</param>
    /// <returns>A list of video variants for the specified file.</returns>
    public async Task<List<VideoVariant>> GetVariantsByFileIdIfCompletedAsync(int fileId)
    {
        return await _dbContext.VideoVariants
            .Where(v => v.VideoFileId == fileId)
            .ToListAsync(); // Filter by file ID and return list
    }
}
