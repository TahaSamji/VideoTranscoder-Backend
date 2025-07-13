



using Microsoft.Extensions.Options;
using VideoTranscoder.VideoTranscoder.Application.Configurations;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;
using VideoTranscoder.VideoTranscoder.Domain.Enums;
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



        public VideoService(ICloudStorageService azureService, IVideoVariantRepository videoVariantRepository, IEncodingProfileRepository encodingProfileRepository, IVideoRepository videoRepository, IMessageQueueService queuePublisher, FFmpegService fFmpegService, IThumbnailRepository thumbnailRepository, IOptions<AzureOptions> azureOptions,
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

        }

        public async Task<List<VideoMetaData>> GetAllVideosByUserIdAsync(int userId, int page, int pageSize)
        {
            return await _videoRepository.GetAllByUserIdAsync(userId, page, pageSize);
        }

        public async Task<List<VideoRenditionDto>> GetVideoRenditionsByFileIdAsync(int fileId)
        {
            var variants = await _videoVariantRepository.GetVariantsByFileIdIfCompletedAsync(fileId);

            return [.. variants.Select(v => new VideoRenditionDto
    {
        VariantId = v.Id,
        Type = v.Type,
        Resolution = v.Resolution,
        BitrateKbps = v.BitrateKbps,
        Size = v.Size,
        DurationSeconds = v.DurationSeconds,
        VideoUrl = v.VideoURL,
        CreatedAt = v.CreatedAt
    })];
        }


        public async Task<string> StoreFileAndReturnThumbnailUrlAsync(int totalChunks, string outputFileName, int userId, long fileSize, int EncodingId)
        {
            try
            {
                // Step 1: Check for existing video
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
                        Status = "Merged",
                        Size = fileSize,
                        CreatedAt = DateTime.UtcNow,
                        defaultThumbnailUrl = "",
                        UpdatedAt = DateTime.UtcNow,
                        
                    };

                    await _videoRepository.SaveAsync(videoMetaData);
                }


                var message = new TranscodeRequestMessage
                {
                    FileId = videoMetaData.Id,
                    BlobPath = videoMetaData.BlobPath,
                    EncodingProfileId = EncodingId
                };
                // EncodingProfile encodingProfile = await _encodingProfileRepository.GetByIdAsync(EncodingId);

                string thumbnailUrl = await GenerateDefaultThumbnailAsync(outputFileName, userId, videoMetaData.Id);
                string queueName = _configuration["AzureServiceBus:TranscodeQueueName"]!;
                await _queuePublisher.SendMessageAsync(message, queueName);

                await _videoRepository.UpdateThumbnailUrlAsync(videoMetaData.Id,thumbnailUrl);
                 await _videoRepository.UpdateStatusAsync(videoMetaData.Id,VideoProcessStatus.Queued.ToString());
                // await _ffmpegService.GenerateMultipleThumbnailsAsync(videoMetaData.OriginalFilename,userId,videoMetaData.Id);
                // await _ffmpegService.TranscodeToCMAFWithCENCAsync(videoMetaData.OriginalFilename, userId,videoMetaData.Id,encodingProfile);
                return thumbnailUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in StoreFileAndReturnThumbnailUrlAsync: " + ex.Message);
                throw;
            }
        }


        private async Task<string> GenerateDefaultThumbnailAsync(string outputFileName, int userId, int videoId)
        {
            try
            {
                // string SASUrl = await _cloudStorageService.GenerateSasUriAsync(outputFileName);
                // Console.WriteLine(SASUrl);
                // var thumbnailStream = await _ffmpegService.GenerateThumbnailAsync(SASUrl, "00:00:05");
                var thumbnailStream = await _ffmpegService.GenerateThumbnailFromDirAsync("00:00:05", outputFileName, userId, videoId);

                // 3. Create thumbnail filename
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(outputFileName);
                var extention = Path.GetExtension(outputFileName);
                var thumbnailFileName = $"{fileNameWithoutExtension}_thumb_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg";

                // 4. Upload thumbnail to blob storage
                var thumbnailBlobPath = await _cloudStorageService.UploadThumbnailAsync(thumbnailStream, thumbnailFileName, videoId, outputFileName);

                // 5. Generate SAS URI for thumbnail access (optional - depends on your needs)
                var thumbnailUrl = _cloudStorageService.GenerateThumbnailSasUri(thumbnailBlobPath);
                Console.WriteLine(thumbnailUrl);

                Console.WriteLine($"✅ Successfully generated and stored thumbnail: {thumbnailBlobPath}");
                var Thumbnail = new Thumbnail
                {
                    FileId = videoId,
                    IsDefault = true,
                    BlobUrl = thumbnailUrl,
                    TimeOffset = "00:00:05",
                    CreatedAt = DateTime.UtcNow

                };

                await _thumbnailRepository.SaveAsync(Thumbnail);

                return thumbnailUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in GenerateAndStoreThumbnailAsync: " + ex.Message);
                throw;
            }
        }




    }

}


// // 3. Upload thumbnail to blob
// // await _blobService.UploadFileAsync(localOutputPath, thumbnailBlobPath);

// // string queueName = _configuration["AzureServiceBus:TranscodeQueueName"]!;
// // await _queuePublisher.SendMessageAsync(message, queueName);

