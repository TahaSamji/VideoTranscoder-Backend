



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
        private readonly IThumbnailService _thumbnailService;
        private readonly IVideoRepository _videoRepository;
        private readonly IMessageQueueService _queuePublisher;
        private readonly IConfiguration _configuration;
        private readonly FFmpegService _ffmpegService;
        private readonly IThumbnailRepository _thumbnailRepository;
        private readonly IVideoVariantRepository _videoVariantRepository;
        private readonly AzureOptions _azureOptions;

        private readonly ILogger<VideoService> _logger;



        public VideoService(ICloudStorageService azureService, IThumbnailService thumbnailService, ILogger<VideoService> logger, IVideoVariantRepository videoVariantRepository, IVideoRepository videoRepository, IMessageQueueService queuePublisher, FFmpegService fFmpegService, IThumbnailRepository thumbnailRepository, IOptions<AzureOptions> azureOptions,
        IConfiguration configuration)
        {
            _cloudStorageService = azureService;
            _videoRepository = videoRepository;
            _configuration = configuration;
            _queuePublisher = queuePublisher;
            _ffmpegService = fFmpegService;
            _thumbnailRepository = thumbnailRepository;
            _azureOptions = azureOptions.Value;
            _videoVariantRepository = videoVariantRepository;
            _logger = logger;
            _thumbnailService = thumbnailService;

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
        public async Task StoreFileAndGenerateThumbnailsAsync(int totalChunks, string outputFileName, int userId, long fileSize, int encodingId)
        {
            try
            {
                _logger.LogInformation("📥 Processing file upload for user {UserId}, file '{FileName}', size {FileSize}", userId, outputFileName, fileSize);

                // Check if video already exists
                var existingVideo = await _videoRepository.FindByNameAndSizeAsync(outputFileName, fileSize, userId);
                if (existingVideo != null)
                {
                    _logger.LogWarning("⚠️ Video already exists for user {UserId}, file '{FileName}', size {FileSize}", userId, outputFileName, fileSize);
                    throw new InvalidOperationException("Video already exists.");
                }

                // Prepare blob path
                string containerName = _azureOptions.ContainerName;
                var blobPath = $"{containerName}/{userId}/{outputFileName}";

                // Create new video metadata entry
                var videoMetaData = new VideoMetaData
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
                _logger.LogInformation("📄 Saved new VideoMetaData with ID {FileId}", videoMetaData.Id);

                // Download video locally to generate thumbnails
                _logger.LogInformation("⬇️ Downloading video locally , FileId {FileId}", videoMetaData.Id);
                string inputFilePath = await _cloudStorageService.DownloadVideoToLocalAsync(outputFileName, userId, videoMetaData.Id);
                // Send transcode request
                var message = new TranscodeRequestMessage
                {
                    FileId = videoMetaData.Id,
                    BlobPath = videoMetaData.BlobPath,
                    EncodingProfileId = encodingId,
                    LocalVideoPath = inputFilePath
                
                };

                string queueName = _configuration["AzureServiceBus:TranscodeQueueName"]!;
                _logger.LogInformation("📤 Sending transcode request to queue '{Queue}' for FileId {FileId}", queueName, videoMetaData.Id);

                await _queuePublisher.SendMessageAsync(message, queueName);
                _logger.LogInformation("✅ Transcode request sent successfully for FileId {FileId}, EncodingId {EncodingId}", videoMetaData.Id, encodingId);
                // Generate and store thumbnails
                _logger.LogInformation("🖼️ Generating thumbnails for FileId {FileId}", videoMetaData.Id);
                var thumbnailUrl = await _thumbnailService.GenerateAndStoreThumbnailsAsync(outputFileName, userId, videoMetaData.Id, inputFilePath);

                // Update thumbnail URL and status
                await _videoRepository.UpdateThumbnailUrlAsync(videoMetaData.Id, thumbnailUrl);
                await _videoRepository.UpdateStatusAsync(videoMetaData.Id, VideoProcessStatus.Queued.ToString());

            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "⚠️ Duplicate upload attempt for user {UserId}, file '{FileName}'", userId, outputFileName);
                throw; // Rethrow to let the caller handle it
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in StoreFileAndReturnThumbnailUrlAsync for user {UserId}, file '{FileName}'", userId, outputFileName);
                throw;
            }
        }









    }

}


// // 3. Upload thumbnail to blob
// // await _blobService.UploadFileAsync(localOutputPath, thumbnailBlobPath);

// // string queueName = _configuration["AzureServiceBus:TranscodeQueueName"]!;
// // await _queuePublisher.SendMessageAsync(message, queueName);

