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


    public ThumbnailService(IMapper thumbnailMapper, LocalCleanerService cleanerService, IVideoRepository videoRepository, ICloudStorageService cloudStorageService, IThumbnailRepository thumbnailRepository, FFmpegService fFmpegService, ILogger<ThumbnailService> logger)
    {
        _thumbnailRepository = thumbnailRepository;
        _logger = logger;
        _ffmpegService = fFmpegService;
        _cloudStorageService = cloudStorageService;
        _thumbnailMapper = thumbnailMapper;
        _videoRepository = videoRepository;
        _cleanerService = cleanerService;

    }
    public async Task<string> GenerateDefaultThumbnailAsync(string outputFileName, int userId, int videoId)
    {
        try
        {
            _logger.LogInformation("🎬 Starting default thumbnail generation for videoId: {VideoId}, userId: {UserId}, fileName: {FileName}", videoId, userId, outputFileName);
            // hardcoded this not recommended
            var thumbnailStream = await _ffmpegService.GenerateThumbnailFromDirAsync("00:00:05", outputFileName, userId, videoId);

            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(outputFileName);
            var extension = Path.GetExtension(outputFileName);
            var thumbnailFileName = $"{fileNameWithoutExtension}_thumb_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg";

            _logger.LogDebug("📝 Thumbnail file name generated: {ThumbnailFileName}", thumbnailFileName);

            var thumbnailBlobPath = await _cloudStorageService.UploadThumbnailAsync(thumbnailStream, thumbnailFileName, videoId, outputFileName);

            var thumbnailUrl = _cloudStorageService.GenerateThumbnailSasUri(thumbnailBlobPath);

            _logger.LogInformation("✅ Thumbnail uploaded to blob storage at path: {BlobPath}", thumbnailBlobPath);
            _logger.LogInformation("🌐 Thumbnail accessible at: {ThumbnailUrl}", thumbnailUrl);

            var thumbnail = new Thumbnail
            {
                FileId = videoId,
                IsDefault = true,
                BlobUrl = thumbnailUrl,
                TimeOffset = "00:00:05",
                CreatedAt = DateTime.UtcNow
            };

            await _thumbnailRepository.SaveAsync(thumbnail);

            _logger.LogInformation("🗄️ Thumbnail metadata saved to DB for videoId: {VideoId}", videoId);

            return thumbnailUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating or saving default thumbnail for videoId: {VideoId}", videoId);
            throw;
        }
    }
    public async Task<string> GenerateAndStoreThumbnailsAsync(string fileName, int userId, int fileId, string filePath)
    {
        string thumbnailPath = await _ffmpegService.GenerateMultipleThumbnailsAsync(userId, fileId, filePath);
        _logger.LogInformation("Thumbnails stored to local successfully {thumbnailPath}:", thumbnailPath);

        List<string> thumbnailBlobPaths = await _cloudStorageService.UploadThumbnailsFromDirectoryAsync(thumbnailPath, fileId, fileName, userId);
        _logger.LogInformation("Thumbnails stored to Blob successfully: {@Paths}", thumbnailBlobPaths);

        var thumbnails = new List<Thumbnail>();

        for (int index = 0; index < thumbnailBlobPaths.Count; index++)
        {
            var blobPath = thumbnailBlobPaths[index];

            string thumnbnailUrl = await _cloudStorageService.GenerateBlobSasUriAsync(blobPath);

            var thumbnail = new Thumbnail
            {
                FileId = fileId,
                BlobUrl = thumnbnailUrl,
                TimeOffset = $"00:00:{(index + 1) * 5:00}",
                IsDefault = false,
                CreatedAt = DateTime.UtcNow
            };

            thumbnails.Add(thumbnail);
        }

        await _thumbnailRepository.SaveAllAsync(thumbnails);
        _logger.LogInformation("✅ Saved {Count} thumbnails to the database with SAS URLs.", thumbnails.Count);
        _logger.LogInformation($"✅ This  is the .{thumbnailPath}");

        var firstThumbnailUrl = thumbnails.FirstOrDefault()?.BlobUrl;

        await _cleanerService.CleanDirectoryContentsAsync(thumbnailPath);

        return firstThumbnailUrl!;
    }

    public async Task<List<ThumbnailDto>> GetAllThumbnailsAsync(int fileId)
    {
        var thumbnails = await _thumbnailRepository.GetAllThumbnailsAsync(fileId);
        return _thumbnailMapper.Map<List<ThumbnailDto>>(thumbnails);
    }

    public async Task SetDefaultThumbnailAsync(int thumbnailId, int fileId)
    {
        // Step 1: Update default flag in thumbnails
        await _thumbnailRepository.SetDefaultThumbnailAsync(thumbnailId);

        // Step 2: Get the new default thumbnail to fetch its URL
        var currentThumbnail = await _thumbnailRepository.GetByIdAsync(thumbnailId);
        if (currentThumbnail == null)
        {
            throw new Exception("Current thumbnail not found after update.");
        }

        // Step 3: Update the default thumbnail URL in the video metadata
        await _videoRepository.UpdateThumbnailUrlAsync(fileId, currentThumbnail.BlobUrl);
    }
    public async Task<int> CountThumbnailsForFileAsync(int fileId)
    {
        return await _thumbnailRepository.CountThumbnailsByFileIdAsync(fileId);
    }
}