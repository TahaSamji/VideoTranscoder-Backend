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
    private readonly IMapper _thumbnailMapper;


    public ThumbnailService(IMapper thumbnailMapper, IVideoRepository videoRepository, ICloudStorageService cloudStorageService, IThumbnailRepository thumbnailRepository, FFmpegService fFmpegService, ILogger<ThumbnailService> logger)
    {
        _thumbnailRepository = thumbnailRepository;
        _logger = logger;
        _ffmpegService = fFmpegService;
        _cloudStorageService = cloudStorageService;
        _thumbnailMapper = thumbnailMapper;
        _videoRepository = videoRepository;

    }
    // public async Task GenerateAndStoreThumbnailsAsync(string fileName, int userId, int fileId)
    // {
    //     string thumbnailPath = await _ffmpegService.GenerateMultipleThumbnailsAsync(fileName, userId, fileId);
    //     List<string> thumbnailBlobPaths = await _cloudStorageService.UploadThumbnailsFromDirectoryAsync(thumbnailPath, fileId, fileName, userId);
    //     _logger.LogInformation("Thumbnails Stored to Blob Succesfully ", thumbnailBlobPaths);
    //     // await _thumbnailRepository.SaveAsync();
    //     // Convert blob URLs to Thumbnail metadata entities
    //     var thumbnails = thumbnailBlobPaths.Select((url, index) =>
       
    //         new Thumbnail
    //         {
    //             FileId = fileId,
    //             BlobUrl = $"https://task1storageaccount.blob.core.windows.net/uploads/{url}",
    //             TimeOffset = $"00:00:{(index + 1) * 5:00}",
    //             IsDefault = false,
    //             CreatedAt = DateTime.UtcNow
    //         }).ToList();

    //     // Save thumbnail metadata to the database
    //     await _thumbnailRepository.SaveAllAsync(thumbnails);

    //     _logger.LogInformation($"✅ Saved {thumbnails.Count} thumbnails to the database.");

    // }
    public async Task GenerateAndStoreThumbnailsAsync(string fileName, int userId, int fileId)
{
    string thumbnailPath = await _ffmpegService.GenerateMultipleThumbnailsAsync(fileName, userId, fileId);
    List<string> thumbnailBlobPaths = await _cloudStorageService.UploadThumbnailsFromDirectoryAsync(thumbnailPath, fileId, fileName, userId);
    
    _logger.LogInformation("Thumbnails stored to Blob successfully: {@Paths}", thumbnailBlobPaths);

    var thumbnails = new List<Thumbnail>();

    for (int index = 0; index < thumbnailBlobPaths.Count; index++)
    {
        var blobPath = thumbnailBlobPaths[index];

        // Generate a SAS URL for this blob path
        string sasUrl = await _cloudStorageService.GenerateBlobSasUriAsync(blobPath);

        var thumbnail = new Thumbnail
        {
            FileId = fileId,
            BlobUrl = sasUrl, // ✅ Use secure SAS URL
            TimeOffset = $"00:00:{(index + 1) * 5:00}",
            IsDefault = false,
            CreatedAt = DateTime.UtcNow
        };

        thumbnails.Add(thumbnail);
    }

    await _thumbnailRepository.SaveAllAsync(thumbnails);
    _logger.LogInformation("✅ Saved {Count} thumbnails to the database with SAS URLs.", thumbnails.Count);
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