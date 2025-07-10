
using System.Diagnostics;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Worker.Services{
    public class TranscodingService : ITranscodingService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly IEncodingProfileRepository _encodingProfileRepository;
        private readonly ITranscodingJobRepository _transcodingJobRepository;
        private readonly FFmpegService _ffmpegService;
        private readonly ILogger<TranscodingService> _logger;
        private readonly ICloudStorageService _cloudStorageService;


        public TranscodingService(
        IVideoRepository videoRepository,
        IEncodingProfileRepository encodingProfileRepository,
        ITranscodingJobRepository transcodingJobRepository,
        FFmpegService ffmpegService,
        ICloudStorageService cloudStorageService,
        ILogger<TranscodingService> logger)
        {
            _videoRepository = videoRepository;
            _encodingProfileRepository = encodingProfileRepository;
            _transcodingJobRepository = transcodingJobRepository;
            _logger = logger;
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
                var existingJob = await _transcodingJobRepository.GetByFileAndProfileAsync(request.FileId, request.EncodingProfileId);
                if (existingJob == null)
                {
                    var newJob = new TranscodingJob
                    {
                        BlobPath = "",
                        VideoFileId = request.FileId,
                        EncodingProfileId = request.EncodingProfileId,
                        Status = "Pending",
                        CreatedAt = DateTime.UtcNow,
                        ErrorMessage = ""

                    };

                    await _transcodingJobRepository.SaveAsync(newJob);
                    _logger.LogInformation("✅ Created new transcoding job (ID: {JobId})", newJob.Id);
                }
                else
                {
                    _logger.LogInformation("ℹ️ Transcoding job already exists (ID: {JobId})", existingJob.Id);
                    return "fail";
                }

                // using var inputStream = await _cloudStorageService.GetBlobStreamAsync(videoFile.OriginalFilename);

                try
                {
                    // await _ffmpegService.TranscodeToCMAFAsync(videoFile.OriginalFilename);
                    // _ffmpegService.TranscodeToHlsAsync(inputStream, "", "");
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