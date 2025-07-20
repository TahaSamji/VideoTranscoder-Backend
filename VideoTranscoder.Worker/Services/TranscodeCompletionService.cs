using VideoTranscoder.VideoTranscoder.Application.enums;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    // Service that checks if all transcoding jobs for a video file are completed.
    // If so, it cleans up the local input directory.
    public class TranscodeCompletionService
    {
        private readonly ILogger<ThumbnailService> _logger; // Logger (shared with ThumbnailService for now)
        private readonly IVideoVariantRepository _videoVariantRepository; // To interact with video variants
        private readonly LocalCleanerService _cleanerService; // Handles cleanup of local directories
        private readonly ITranscodingJobRepository _transcodingJobRepository; // Handles job metadata
        private readonly IVideoRepository _videoRepository;

        public TranscodeCompletionService(
            LocalCleanerService cleanerService,
            ITranscodingJobRepository transcodingJobRepository,
            IVideoVariantRepository videoVariantRepository,
            IVideoRepository videoRepository,
            ILogger<ThumbnailService> logger)
        {
            _logger = logger;
            _cleanerService = cleanerService;
            _videoVariantRepository = videoVariantRepository;
            _transcodingJobRepository = transcodingJobRepository;
            _videoRepository = videoRepository;
        }

        /// <summary>
        /// Checks if all expected transcoding jobs for a video file are completed.
        /// If yes, cleans up the local directory containing the input file.
        /// </summary>
        /// <param name="totalVariantCount">Expected number of renditions/variants</param>
        /// <param name="fileId">ID of the original video file</param>
        /// <param name="inputFilePath">Path to the input video file</param>
        public async Task CheckAndCleanIfAllJobsFinishedAsync(int totalVariantCount, int fileId, string inputFilePath,int userId)

        {
            
            // Get the number of completed transcoding jobs for the file
            var completedJobCount = await _transcodingJobRepository.CountFinishedJobsByFileIdAsync(fileId);

            // If all expected variants are completed, clean up local directory
            if (completedJobCount == totalVariantCount)
            {
                _logger.LogInformation(
                    "✅ All {Count} transcoding jobs for FileId {FileId} are complete. Cleaning local input directory...",
                    completedJobCount, fileId
                );
                // Update Video Status to Completed
                await _videoRepository.UpdateStatusAsync(fileId, VideoProcessStatus.Completed.ToString());
                //Clean output Dir
                string currentDir = Directory.GetCurrentDirectory();
                var outputDir = Path.Combine(currentDir, "temp", $"{userId}", $"{fileId}");
                // Get parent directory of the video file path (e.g., .../input/userId/fileId/videos)
                string? parentDirectory = Directory.GetParent(inputFilePath)?.FullName;

                // Clean all files inside the parent directory
                await _cleanerService.CleanDirectoryContentsAsync(outputDir);
                await _cleanerService.CleanDirectoryContentsAsync(parentDirectory!);
            }
            else
            {
                // Log current progress if not all jobs are completed
                _logger.LogInformation(
                    "⏳ Transcoding progress for FileId {FileId}: {Completed}/{Total} jobs completed.",
                    fileId, completedJobCount, totalVariantCount
                );
            }
        }
    }
}
