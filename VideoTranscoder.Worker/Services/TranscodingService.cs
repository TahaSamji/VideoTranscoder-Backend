
using System.Diagnostics;
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
        private readonly ILogger<TranscodingService> _logger;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly IVideoVariantRepository _videoVariantRepository;


        public TranscodingService(
        IVideoRepository videoRepository,
        IEncodingProfileRepository encodingProfileRepository,
        ITranscodingJobRepository transcodingJobRepository,
        FFmpegService ffmpegService,
        ICloudStorageService cloudStorageService,
         IVideoVariantRepository videoVariantRepository,
        ILogger<TranscodingService> logger)
        {
            _videoRepository = videoRepository;
            _encodingProfileRepository = encodingProfileRepository;
            _transcodingJobRepository = transcodingJobRepository;
            _logger = logger;
            _videoVariantRepository = videoVariantRepository;
            _ffmpegService = ffmpegService;
            _cloudStorageService = cloudStorageService;
        }

        public async Task<string> TranscodeVideoAsync(TranscodeRequestMessage request, CancellationToken cancellationToken = default)
        {
            var stopwatch = Stopwatch.StartNew();

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
                        Status = "Pending",
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
                    string outputfile = "";
                    if (encodingProfile.FormatType == "hls")
                    {
                        outputfile = "playlist.m3u8";
                    }
                    else
                    {
                        outputfile = "manifest.mpd";
                    }
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
                        VideoURL = $"https://task1storageaccount.blob.core.windows.net/uploads/{videoBlobpath}/{encodingProfile.FormatType}/{outputfile}" // or manifest.mpd
                    };

                    await _videoVariantRepository.SaveAsync(variant);
                    _logger.LogInformation("🎞️ Video variant saved.");
                    await _ffmpegService.GenerateMultipleThumbnailsAsync(videoFile.OriginalFilename,videoFile.UserId,videoFile.Id);


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