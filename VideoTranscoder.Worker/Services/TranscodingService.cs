
using System.Diagnostics;
using Microsoft.Extensions.Options;
using VideoTranscoder.VideoTranscoder.Application.Configurations;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    public class TranscodingService : ITranscodingService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IEncodingProfileRepository _encodingProfileRepository;
        private readonly ITranscodingJobRepository _transcodingJobRepository;
        private readonly FFmpegService _ffmpegService;
        private readonly AzureOptions _azureOptions;
        private readonly IThumbnailService _thumbnailService;
        private readonly ILogger<TranscodingService> _logger;
        private readonly ICDNService _CDNService;
        private readonly LocalCleanerService _cleanerService;
        private readonly IVideoVariantRepository _videoVariantRepository;


        public TranscodingService(
        IVideoRepository videoRepository,
        IEncodingProfileRepository encodingProfileRepository,
        ITranscodingJobRepository transcodingJobRepository,
        FFmpegService ffmpegService,
        IOptions<AzureOptions> azureOptions,
        IThumbnailService thumbnailService,
        ICloudStorageService cloudStorageService,
         IVideoVariantRepository videoVariantRepository,
         ICDNService CDNService,
        LocalCleanerService cleanerService,
        ILogger<TranscodingService> logger)
        {
            _videoRepository = videoRepository;
            _encodingProfileRepository = encodingProfileRepository;
            _transcodingJobRepository = transcodingJobRepository;
            _logger = logger;
            _videoVariantRepository = videoVariantRepository;
            _ffmpegService = ffmpegService;
            _CDNService = CDNService;
            _thumbnailService = thumbnailService;
            _azureOptions = azureOptions.Value;
            _cleanerService = cleanerService;

        }

        public async Task<string> TranscodeVideoAsync(TranscodeRequestMessage request, CancellationToken cancellationToken = default)
        {


            try
            {
                _logger.LogInformation("Starting transcoding for FileId: {FileId}", request.FileId);

                var videoFile = await _videoRepository.GetByIdAsync(request.FileId);
                var encodingProfile = await _encodingProfileRepository.GetByIdAsync(request.EncodingProfileId);

                if (videoFile == null || encodingProfile == null)
                {
                    throw new InvalidOperationException("Video file or encoding profile not found");
                }

                // === CREATE JOB FOR IDEMPOTENCY ===
                var Job = await _transcodingJobRepository.GetByFileAndProfileAsync(request.FileId, request.EncodingProfileId);
                if (Job == null)
                {
                    Job = new TranscodingJob
                    {
                        BlobPath = "",
                        VideoFileId = request.FileId,
                        EncodingProfileId = request.EncodingProfileId,
                        Status = "Queued",
                        CreatedAt = DateTime.UtcNow,
                        ErrorMessage = ""

                    };

                    await _transcodingJobRepository.SaveAsync(Job);
                    _logger.LogInformation("✅ Created new transcoding job (ID: {JobId})", Job.Id);
                }
                else
                {
                    _logger.LogInformation("ℹ️ Transcoding job already exists (ID: {JobId})", Job.Id);
                    return "fail";
                }

                // using var inputStream = await _cloudStorageService.GetBlobStreamAsync(videoFile.OriginalFilename);

                try
                {
                    string videoBlobpath = await _ffmpegService.TranscodeToCMAFAsync(videoFile.OriginalFilename, videoFile.UserId, videoFile.Id, encodingProfile);

                    await _transcodingJobRepository.UpdateStatusAsync(Job.Id, "Completed");
                    string outputfile = "";
                    if (encodingProfile.FormatType == "hls")
                    {
                        outputfile = "playlist.m3u8";
                    }
                    else
                    {
                        outputfile = "manifest.mpd";
                    }
                    string storagePath = $"{_azureOptions.ContainerName}/{videoBlobpath}/{encodingProfile.FormatType}/{outputfile}";
                    string videoUrl = await _CDNService.GenerateSignedUrlAsync(storagePath);
                    var variant = new VideoVariant
                    {

                        TranscodingJobId = Job.Id,
                        Type = encodingProfile.FormatType,
                        BlobPath = videoBlobpath, // or manifest.mpd
                        Resolution = encodingProfile.Resolution,
                        BitrateKbps = encodingProfile.Bitrate,
                        Size = 0, // Optional: use ICloudStorageService to get blob size
                        DurationSeconds = 0, // Optional: extract from metadata if needed
                        CreatedAt = DateTime.UtcNow,
                        VideoURL = videoUrl // or manifest.mpd
                    };

                    await _videoVariantRepository.SaveAsync(variant);
                    _logger.LogInformation("🎞️ Video variant saved.");
                    int thumbnailCount = await _thumbnailService.CountThumbnailsForFileAsync(videoFile.Id);
                    if (thumbnailCount < 6)
                    {
                        await _thumbnailService.GenerateAndStoreThumbnailsAsync(videoFile.OriginalFilename, videoFile.UserId, videoFile.Id);
                        _logger.LogInformation("Thumbnails Saved .");
                    }

                    await _cleanerService.CleanDirectoryContentsAsync("temp");
                    await _cleanerService.CleanDirectoryContentsAsync("input");

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    throw;
                }


                return "Success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during transcoding");
                return "Failed";
            }
        }
    }

}