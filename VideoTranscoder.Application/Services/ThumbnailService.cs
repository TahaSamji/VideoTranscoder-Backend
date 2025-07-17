using AutoMapper;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;
using VideoTranscoder.VideoTranscoder.Worker.Services;

public class ThumbnailService : IThumbnailService
{
    private readonly FFmpegService _ffmpegService;
    private readonly ILogger<ThumbnailService> _logger;
    private readonly IThumbnailRepository _thumbnailRepository;
    private readonly IVideoRepository _videoRepository;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly LocalCleanerService _cleanerService;
    private readonly IMapper _thumbnailMapper;

    public ThumbnailService(
        IMapper thumbnailMapper,
        LocalCleanerService cleanerService,
        IVideoRepository videoRepository,
        ICloudStorageService cloudStorageService,
        IThumbnailRepository thumbnailRepository,
        FFmpegService fFmpegService,
        ILogger<ThumbnailService> logger)
    {
        _thumbnailRepository = thumbnailRepository;
        _logger = logger;
        _ffmpegService = fFmpegService;
        _cloudStorageService = cloudStorageService;
        _thumbnailMapper = thumbnailMapper;
        _videoRepository = videoRepository;
        _cleanerService = cleanerService;
    }

    public async Task<string> GenerateAndStoreThumbnailsAsync(string fileName, int userId, int fileId, string filePath)
    {
        // 1. Generate thumbnails locally using FFmpeg
        string thumbnailPath = await _ffmpegService.GenerateMultipleThumbnailsAsync(userId, fileId, filePath);
        _logger.LogInformation("Thumbnails stored to local successfully {thumbnailPath}:", thumbnailPath);

        // 2. Upload generated thumbnails to Azure Blob Storage
        List<string> thumbnailBlobPaths = await _cloudStorageService.UploadThumbnailsFromDirectoryAsync(thumbnailPath, fileId, fileName, userId);
        _logger.LogInformation("Thumbnails stored to Blob successfully: {@Paths}", thumbnailBlobPaths);

        var thumbnails = new List<Thumbnail>();

        // 3. Construct thumbnail entities and generate signed SAS URLs
        for (int index = 0; index < thumbnailBlobPaths.Count; index++)
        {
            var blobPath = thumbnailBlobPaths[index];

            // Generate secure URL for thumbnail blob
            string thumnbnailUrl = await _cloudStorageService.GenerateBlobSasUriAsync(blobPath);

            // Create thumbnail entity
            var thumbnail = new Thumbnail
            {
                FileId = fileId,
                BlobUrl = thumnbnailUrl,
                TimeOffset = $"00:00:{(index + 1) * 5:00}", // e.g., 5, 10, 15 seconds, etc.
                IsDefault = false,
                CreatedAt = DateTime.UtcNow
            };

            thumbnails.Add(thumbnail);
        }

        // 4. Save all thumbnail entries to the database
        await _thumbnailRepository.SaveAllAsync(thumbnails);
        _logger.LogInformation("✅ Saved {Count} thumbnails to the database with SAS URLs.", thumbnails.Count);

        var firstThumbnailUrl = thumbnails.FirstOrDefault()?.BlobUrl;

        // 5. Clean the local temporary thumbnail directory
        await _cleanerService.CleanDirectoryContentsAsync(thumbnailPath);

        return firstThumbnailUrl!;
    }

    public async Task<List<ThumbnailDto>> GetAllThumbnailsAsync(int fileId)
    {
        // Fetch all thumbnail records for the given file
        var thumbnails = await _thumbnailRepository.GetAllThumbnailsAsync(fileId);

        // Map them to DTOs before returning
        return _thumbnailMapper.Map<List<ThumbnailDto>>(thumbnails);
    }

    public async Task SetDefaultThumbnailAsync(int thumbnailId, int fileId)
    {
        // Step 1: Update the IsDefault flag in thumbnails
        await _thumbnailRepository.SetDefaultThumbnailAsync(thumbnailId);

        // Step 2: Fetch the selected thumbnail to retrieve its URL
        var currentThumbnail = await _thumbnailRepository.GetByIdAsync(thumbnailId);
        if (currentThumbnail == null)
        {
            throw new NotFoundException("Current thumbnail not found after update.");
        }

        // Step 3: Update the default thumbnail URL in the video metadata
        await _videoRepository.UpdateThumbnailUrlAsync(fileId, currentThumbnail.BlobUrl);
    }

    public async Task<int> CountThumbnailsForFileAsync(int fileId)
    {
        // Return the total number of thumbnails for a given video file
        return await _thumbnailRepository.CountThumbnailsByFileIdAsync(fileId);
    }
}
