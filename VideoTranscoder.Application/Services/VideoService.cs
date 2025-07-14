



using Microsoft.Extensions.Options;
using VideoTranscoder.VideoTranscoder.Application.Configurations;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.enums;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;
using VideoTranscoder.VideoTranscoder.Worker.Services;

namespace VideoTranscoder.VideoTranscoder.Application.Services
{
    public class VideoService : IVideoService
    {
        private readonly ICloudStorageService _cloudStorageService;
        private readonly IVideoRepository _videoRepository;
        private readonly IMessageQueueService _queuePublisher;
        private readonly IConfiguration _configuration;
        private readonly FFmpegService _ffmpegService;
        private readonly IThumbnailRepository _thumbnailRepository;
        private readonly IVideoVariantRepository _videoVariantRepository;
        private readonly AzureOptions _azureOptions;
        private readonly IEncodingProfileRepository _encodingProfileRepository;
        private readonly ILogger<VideoService> _logger;



        public VideoService(ICloudStorageService azureService, ILogger<VideoService> logger, IVideoVariantRepository videoVariantRepository, IEncodingProfileRepository encodingProfileRepository, IVideoRepository videoRepository, IMessageQueueService queuePublisher, FFmpegService fFmpegService, IThumbnailRepository thumbnailRepository, IOptions<AzureOptions> azureOptions,
        IConfiguration configuration)
        {
            _cloudStorageService = azureService;
            _videoRepository = videoRepository;
            _configuration = configuration;
            _queuePublisher = queuePublisher;
            _ffmpegService = fFmpegService;
            _thumbnailRepository = thumbnailRepository;
            _azureOptions = azureOptions.Value;
            _encodingProfileRepository = encodingProfileRepository;
            _videoVariantRepository = videoVariantRepository;
            _logger = logger;

        }

        public async Task<List<VideoMetaData>> GetAllVideosByUserIdAsync(int userId, int page, int pageSize)
        {
            return await _videoRepository.GetAllByUserIdAsync(userId, page, pageSize);
        }

        public async Task<List<VideoRenditionDto>> GetVideoRenditionsByFileIdAsync(int fileId)
        {
            try
            {
                _logger.LogInformation("🔍 Fetching video renditions for fileId: {FileId}", fileId);

                var variants = await _videoVariantRepository.GetVariantsByFileIdIfCompletedAsync(fileId);

                if (variants == null || !variants.Any())
                {
                    _logger.LogWarning("⚠️ No completed variants found for fileId: {FileId}", fileId);
                    return new List<VideoRenditionDto>();
                }

                var renditionDtos = variants.Select(v => new VideoRenditionDto
                {
                    VariantId = v.Id,
                    Type = v.Type,
                    Resolution = v.Resolution,
                    BitrateKbps = v.BitrateKbps,
                    Size = v.Size,
                    DurationSeconds = v.DurationSeconds,
                    VideoUrl = v.VideoURL,
                    CreatedAt = v.CreatedAt
                }).ToList();

                _logger.LogInformation("✅ Found {Count} renditions for fileId: {FileId}", renditionDtos.Count, fileId);

                return renditionDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error while fetching video renditions for fileId: {FileId}", fileId);
                throw;
            }
        }


        public async Task<string> StoreFileAndReturnThumbnailUrlAsync(int totalChunks, string outputFileName, int userId, long fileSize, int encodingId)
        {
            try
            {
                _logger.LogInformation("📥 Processing file upload for user {UserId}, file '{FileName}', size {FileSize}", userId, outputFileName, fileSize);

                string thumbnailUrl;
                var videoMetaData = await _videoRepository.FindByNameAndSizeAsync(outputFileName, fileSize, userId);

                if (videoMetaData == null)
                {
                    string containerName = _azureOptions.ContainerName;
                    var blobPath = $"{containerName}/{userId}/{outputFileName}";

                    videoMetaData = new VideoMetaData
                    {
                        UserId = userId,
                        OriginalFilename = outputFileName,
                        BlobPath = blobPath,
                        Status = VideoProcessStatus.Merged.ToString(),
                        Size = fileSize,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        defaultThumbnailUrl = string.Empty
                    };

                    await _videoRepository.SaveAsync(videoMetaData);
                    _logger.LogInformation("📄 Saved new VideoMetaData for fileId {FileId}", videoMetaData.Id);

                    _logger.LogInformation("🖼️ Generating thumbnail for video ID {FileId}", videoMetaData.Id);
                    thumbnailUrl = await GenerateDefaultThumbnailAsync(outputFileName, userId, videoMetaData.Id);

                    await _videoRepository.UpdateThumbnailUrlAsync(videoMetaData.Id, thumbnailUrl);
                    await _videoRepository.UpdateStatusAsync(videoMetaData.Id, VideoProcessStatus.Queued.ToString());
                }
                else
                {
                    _logger.LogInformation("ℹ️ Existing metadata found for file '{FileName}'", outputFileName);
                    _logger.LogInformation("ℹ️ Skipping thumbnail generation for file '{FileName}'", outputFileName);
                    thumbnailUrl = videoMetaData.defaultThumbnailUrl;
                }
                string currentDir = Directory.GetCurrentDirectory();
                string inputDir = Path.Combine(currentDir, "input", $"{userId}", $"{videoMetaData.Id}", "videos");
                string localFilePath = Path.Combine(inputDir, outputFileName);
                FileUsageTracker.Increment(localFilePath);

                var message = new TranscodeRequestMessage
                {
                    FileId = videoMetaData.Id,
                    BlobPath = videoMetaData.BlobPath,
                    EncodingProfileId = encodingId
                };

                string queueName = _configuration["AzureServiceBus:TranscodeQueueName"]!;
                _logger.LogInformation("📤 Sending transcode request to queue '{Queue}' for fileId {FileId}", queueName, videoMetaData.Id);

                await _queuePublisher.SendMessageAsync(message, queueName);
                _logger.LogInformation("✅ Video file {FileId} processing queued successfully  : {Encoding Id}", videoMetaData.Id, encodingId);

                return thumbnailUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in StoreFileAndReturnThumbnailUrlAsync for user {UserId} and file '{FileName}'", userId, outputFileName);
                throw;
            }
        }



        private async Task<string> GenerateDefaultThumbnailAsync(string outputFileName, int userId, int videoId)
        {
            try
            {
                _logger.LogInformation("🎬 Starting default thumbnail generation for videoId: {VideoId}, userId: {UserId}, fileName: {FileName}", videoId, userId, outputFileName);

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



    }

}


// // 3. Upload thumbnail to blob
// // await _blobService.UploadFileAsync(localOutputPath, thumbnailBlobPath);

// // string queueName = _configuration["AzureServiceBus:TranscodeQueueName"]!;
// // await _queuePublisher.SendMessageAsync(message, queueName);

