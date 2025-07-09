



using VideoTranscoder.VideoTranscoder.Application.DTOs;
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


        public VideoService(ICloudStorageService azureService, IVideoRepository videoRepository, IMessageQueueService queuePublisher, FFmpegService fFmpegService, IThumbnailRepository thumbnailRepository,
        IConfiguration configuration)
        {
            _cloudStorageService = azureService;
            _videoRepository = videoRepository;
            _configuration = configuration;
            _queuePublisher = queuePublisher;
            _ffmpegService = fFmpegService;
            _thumbnailRepository = thumbnailRepository;
        }

        public async Task<string> StoreFileAndReturnThumbnailUrlAsync(int totalChunks, string outputFileName, int userId, long fileSize, int EncodingId)
        {
            try
            {
                // 1. Construct Blob path
                var blobPath = $"uploads/{outputFileName}";

                // 2. Save metadata in DB
                var videoMetaData = new VideoMetaData
                {

                    UserId = userId,
                    OriginalFilename = outputFileName,
                    BlobPath = blobPath,
                    Status = "Merged",
                    Size = fileSize,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    EncodingProfileId = EncodingId
                };


                await _videoRepository.SaveAsync(videoMetaData);
                var message = new TranscodeRequestMessage
                {
                    FileId = videoMetaData.Id,
                    BlobPath = videoMetaData.BlobPath,  // $"uploads/{outputFileName}"
                    EncodingProfileId = 9
                };

                // string queueName = _configuration["AzureServiceBus:TranscodeQueueName"]!;
                // await _queuePublisher.SendMessageAsync(message, queueName);

                // 3. Generate thumbnail
                // string thumbnailUrl = await GenerateAndStoreThumbnailAsync(outputFileName, userId, videoMetaData.Id);

                return "thumbnailUrl";
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in StoreFileAndReturnThumbnailUrlAsync: " + ex.Message);
                throw;
            }
        }

        // private async Task<string> GenerateAndStoreThumbnailAsync(string outputFileName, int userId, int videoId)
        // {
        //     try
        //     {
        //         // 1. Get video blob as stream
        //         var videoBlobPath = $"uploads/{outputFileName}";
        //         Console.WriteLine("blob PAth", videoBlobPath);
        //         var videoStream = await _cloudStorageService.GetBlobStreamAsync(outputFileName);
            

        //         // 2. Generate thumbnail using FFmpeg
        //         var thumbnailStream = await _ffmpegService.GenerateThumbnailAsync(videoBlobPath, "00:00:05");

        //         // 3. Create thumbnail filename
        //         var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(outputFileName);
        //         var thumbnailFileName = $"{fileNameWithoutExtension}_thumb_{userId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.jpg";

        //         // 4. Upload thumbnail to blob storage
        //         var thumbnailBlobPath = await _cloudStorageService.UploadThumbnailAsync(thumbnailStream, thumbnailFileName);

        //         // 5. Generate SAS URI for thumbnail access (optional - depends on your needs)
        //         var thumbnailUrl = _cloudStorageService.GenerateThumbnailSasUri(thumbnailBlobPath);

        //         Console.WriteLine($"✅ Successfully generated and stored thumbnail: {thumbnailBlobPath}");

        //         var Thumbnail = new Thumbnail
        //         {
        //             FileId = videoId,
        //             IsDefault = true,
        //             BlobUrl = thumbnailBlobPath,
        //             TimeOffset = "00:00:05",
        //             CreatedAt = DateTime.UtcNow



        //         };

        //         await _thumbnailRepository.SaveAsync(Thumbnail);


        //         return thumbnailUrl;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("❌ Error in GenerateAndStoreThumbnailAsync: " + ex.Message);
        //         throw;
        //     }
        // }

    }
    
}


// // 3. Upload thumbnail to blob
// // await _blobService.UploadFileAsync(localOutputPath, thumbnailBlobPath);

// // string queueName = _configuration["AzureServiceBus:TranscodeQueueName"]!;
// // await _queuePublisher.SendMessageAsync(message, queueName);

