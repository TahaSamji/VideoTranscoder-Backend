



using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.Tsp;
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
        private readonly IEncodingProfileRepository _encodingProfileRepository;
        private readonly IVideoVariantRepository _videoVariantRepository;
        private readonly AzureOptions _azureOptions;

        private readonly ILogger<VideoService> _logger;



        public VideoService(IEncodingProfileRepository encodingProfileRepository, ICloudStorageService azureService, IThumbnailService thumbnailService, ILogger<VideoService> logger, IVideoVariantRepository videoVariantRepository, IVideoRepository videoRepository, IMessageQueueService queuePublisher, FFmpegService fFmpegService, IOptions<AzureOptions> azureOptions,
        IConfiguration configuration)
        {
            _cloudStorageService = azureService;
            _videoRepository = videoRepository;
            _configuration = configuration;
            _queuePublisher = queuePublisher;
            _ffmpegService = fFmpegService;
            _encodingProfileRepository = encodingProfileRepository;
            _azureOptions = azureOptions.Value;
            _videoVariantRepository = videoVariantRepository;
            _logger = logger;
            _thumbnailService = thumbnailService;

        }

        public async Task<List<VideoMetaData>> GetAllVideosByUserIdAsync(int userId, int page, int pageSize)
        {
            // Fetch videos uploaded by the user
            var videos = await _videoRepository.GetAllByUserIdAsync(userId, page, pageSize);
            return videos;
        }
        public async Task<List<VideoRenditionDto>> GetVideoRenditionsByFileIdAsync(int fileId)
        {
            try
            {
                // Log the beginning of the rendition fetch process
                _logger.LogInformation("🔍 Fetching video renditions for fileId: {FileId}", fileId);

                // Fetch all completed video variants (renditions) for the given fileId
                var variants = await _videoVariantRepository.GetVariantsByFileIdIfCompletedAsync(fileId);

                // If no variants found, log a warning and return an empty list
                if (variants == null || !variants.Any())
                {
                    _logger.LogWarning("⚠️ No completed variants found for fileId: {FileId}", fileId);
                    return [];
                }

                // Convert variants into DTOs for response
                var renditionDtos = variants.Select(v => new VideoRenditionDto
                {
                    VariantId = v.Id,
                    Type = v.Type,
                    Resolution = v.Resolution,
                    BitrateKbps = v.BitrateKbps,
                    DurationSeconds = v.DurationSeconds,
                    VideoUrl = v.VideoURL,
                    CreatedAt = v.CreatedAt
                }).ToList();

                // Log success and count of found renditions
                _logger.LogInformation("✅ Found {Count} renditions for fileId: {FileId}", renditionDtos.Count, fileId);

                return renditionDtos;
            }
            catch (Exception ex)
            {
                // Log any exception that occurs during processing
                _logger.LogError(ex, "❌ Error while fetching video renditions for fileId: {FileId}", fileId);
                throw;
            }
        }

        public async Task StoreFileAndGenerateThumbnailsAsync(MergeRequestDto request, int userId)
        {
            try
            {
                _logger.LogInformation("📥 Processing file upload for user {UserId}, file '{FileName}', size {FileSize}", userId, request.OutputFileName, request.FileSize);

                // Check if video already exists
                var existingVideo = await _videoRepository.FindByNameAndSizeAsync(request.OutputFileName, request.FileSize, userId);
                if (existingVideo != null)
                {
                    _logger.LogWarning("⚠️ Video already exists for user {UserId}, file '{FileName}', size {FileSize}", userId, request.OutputFileName, request.FileSize);
                    throw new VideoAlreadyExistsException("Video already exists.");
                }

                // Prepare blob path
                string containerName = _azureOptions.ContainerName;
                var blobPath = $"{containerName}/{userId}/{request.OutputFileName}";

                // Create new video metadata entry
                var videoMetaData = new VideoMetaData
                {
                    UserId = userId,
                    OriginalFilename = request.OutputFileName,
                    Resolution = request.Resolution,
                    BlobPath = blobPath,
                    Status = VideoProcessStatus.Merged.ToString(),
                    Size = request.FileSize,
                    TotalChunks = request.TotalChunks,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    Duration = request.Duration,
                    MIMEType = request.MIMEType,
                    Height = request.Height,
                    Width = request.Width,
                    defaultThumbnailUrl = string.Empty
                };

                await _videoRepository.SaveAsync(videoMetaData);
                _logger.LogInformation("📄 Saved new VideoMetaData with ID {FileId}", videoMetaData.Id);

                // Download video locally to generate thumbnails
                _logger.LogInformation("⬇️ Downloading video locally , FileId {FileId}", videoMetaData.Id);
                string inputFilePath = await _cloudStorageService.DownloadVideoToLocalAsync(request.OutputFileName, userId, videoMetaData.Id);
                // Send transcode request
                //  Fetch all matching encoding profiles by height
                var encodingProfiles = await _encodingProfileRepository.GetProfilesUpToHeightAndBrowserTypeAsync(request.Height, request.BrowserType);

                if (!encodingProfiles.Any())
                {
                    _logger.LogWarning("⚠️ No encoding profiles matched for height {Height}", request.Height);
                    throw new NotFoundException("No valid encoding profiles found.");
                }

                //  Prepare all messages
                var messages = encodingProfiles.Select(profile => new TranscodeRequestMessage
                {
                    FileId = videoMetaData.Id,
                    EncodingProfileId = profile.Id,
                    LocalVideoPath = inputFilePath,
                    TotalRenditions = encodingProfiles.Count
                }).ToList();

                //  Send as batch
                string queueName = _configuration["AzureServiceBus:TranscodeQueueName"]!;
                await _queuePublisher.SendBatchAsync(messages, queueName);

                _logger.LogInformation("✅ Sent {Count} transcode requests for FileId {FileId} to queue '{Queue}'", messages.Count, videoMetaData.Id, queueName);

                //  Generate and update thumbnail
                // Generate and store thumbnails
                _logger.LogInformation("🖼️ Generating thumbnails for FileId {FileId}", videoMetaData.Id);
                var thumbnailUrl = await _thumbnailService.GenerateAndStoreThumbnailsAsync(request.OutputFileName, userId, videoMetaData.Id, inputFilePath);

                // Update thumbnail URL and status
                await _videoRepository.UpdateThumbnailUrlAsync(videoMetaData.Id, thumbnailUrl);
                await _videoRepository.UpdateStatusAsync(videoMetaData.Id, VideoProcessStatus.Queued.ToString());

            }
            catch (VideoAlreadyExistsException ex)
            {
                _logger.LogWarning(ex, "⚠️ Duplicate upload attempt for user {UserId}, file '{FileName}'", userId, request.OutputFileName);
                throw; // Rethrow to let the caller handle it
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in StoreFileAndReturnThumbnailUrlAsync for user {UserId}, file '{FileName}'", userId, request.OutputFileName);
                throw;
            }
        }









    }

}
