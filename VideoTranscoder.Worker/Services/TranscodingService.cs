
using System.Diagnostics;
using Microsoft.Extensions.Options;
using VideoTranscoder.VideoTranscoder.Application.Configurations;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.enums;
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
        private readonly TranscodeCompletionService _completionService;
        private readonly IVideoVariantRepository _videoVariantRepository;


        public TranscodingService(
        IVideoRepository videoRepository,
        IEncodingProfileRepository encodingProfileRepository,
        ITranscodingJobRepository transcodingJobRepository,
        FFmpegService ffmpegService,
        IOptions<AzureOptions> azureOptions,
        IThumbnailService thumbnailService,
         IVideoVariantRepository videoVariantRepository,
         ICDNService CDNService,
          TranscodeCompletionService completionService,
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
            _completionService = completionService;

        }
        public async Task<string> TranscodeVideoAsync(TranscodeRequestMessage request, CancellationToken cancellationToken = default)
{
    _logger.LogInformation("📽️ Starting transcoding for FileId: {FileId}", request.FileId);

    try
    {
        // Fetch video metadata and encoding profile
        var videoFile = await _videoRepository.GetByIdAsync(request.FileId);
        var encodingProfile = await _encodingProfileRepository.GetByIdAsync(request.EncodingProfileId);

        if (videoFile == null || encodingProfile == null)
        {
            _logger.LogWarning("⚠️ Video file or encoding profile not found for FileId: {FileId}, ProfileId: {ProfileId}", request.FileId, request.EncodingProfileId);
            throw new NotFoundException("Video file or encoding profile not found.");
        }

        // Check for existing job to ensure idempotency
        var job = await _transcodingJobRepository.GetByFileAndProfileAsync(request.FileId, request.EncodingProfileId);
        if (job != null)
        {
            _logger.LogInformation("ℹ️ Transcoding job already exists (JobId: {JobId}) for FileId: {FileId}", job.Id, request.FileId);
            return "fail";
        }

        // Create new job record
        job = new TranscodingJob
        {
            VideoFileId = request.FileId,
            EncodingProfileId = request.EncodingProfileId,
            Status = VideoProcessStatus.InProgress.ToString(),
            CreatedAt = DateTime.UtcNow,
           
        };

        await _transcodingJobRepository.SaveAsync(job);
        _logger.LogInformation("✅ New transcoding job created (JobId: {JobId})", job.Id);

        try
        {
            // Begin transcoding using FFmpeg
            string videoBlobPath = await _ffmpegService.TranscodeToCMAFAsync(
                request.LocalVideoPath,
                videoFile.OriginalFilename,
                videoFile.UserId,
                videoFile.Id,
                encodingProfile
            );

            // Mark job as complete
            await _transcodingJobRepository.UpdateStatusAsync(job.Id, VideoProcessStatus.Completed.ToString());

            // Build output path for manifest
            string outputFileName = encodingProfile.FormatType == "hls" ? "playlist.m3u8" : "manifest.mpd";
            string storagePath = $"{_azureOptions.ContainerName}/{videoBlobPath}/{encodingProfile.FormatType}/{outputFileName}";

            // Generate signed CDN URL
            string videoUrl = await _CDNService.GenerateSignedUrlAsync(storagePath);

            // Save video variant
            var variant = new VideoVariant
            {
                VideoFileId = videoFile.Id,
                TranscodingJobId = job.Id,
                Type = encodingProfile.FormatType,
                BlobPath = videoBlobPath,
                Resolution = encodingProfile.Resolution,
                BitrateKbps = encodingProfile.Bitrate,
                DurationSeconds = videoFile.Duration,
                CreatedAt = DateTime.UtcNow,
                VideoURL = videoUrl
            };

            await _videoVariantRepository.SaveAsync(variant);
            _logger.LogInformation("🎞️ Video variant saved for FileId: {FileId}, JobId: {JobId}", videoFile.Id, job.Id);

            // Check if all renditions are complete and trigger cleanup
            await _completionService.CheckAndCleanIfAllJobsFinishedAsync(request.TotalRenditions, videoFile.Id, request.LocalVideoPath);
        }
        catch (Exception transcodeEx)
        {
            _logger.LogError(transcodeEx, "❌ Transcoding failed for FileId: {FileId}, JobId: {JobId}", request.FileId, job.Id);

            // Update job with failure status and error message
            await _transcodingJobRepository.UpdateErrorStatusAsync(job.Id, VideoProcessStatus.Failed.ToString(), transcodeEx.Message);

            // Propagate the error up
            throw;
        }

        return "Success";
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Unexpected error occurred while processing FileId: {FileId}", request.FileId);
        return "Failed";
    }
}


    }

}