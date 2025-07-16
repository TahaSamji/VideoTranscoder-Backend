using System.Diagnostics;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    public class FFmpegService
    {
        private readonly ILogger<FFmpegService> _logger;
        private readonly ICloudStorageService _cloudStorageService;
        private readonly LocalCleanerService _cleanerService;




        public FFmpegService(ILogger<FFmpegService> logger, LocalCleanerService cleanerService, ICloudStorageService cloudStorageService)
        {
            _logger = logger;
            _cloudStorageService = cloudStorageService;
            _cleanerService = cleanerService;
        }
        public async Task<string> TranscodeToCMAFAsync(string filePath, string fileName, int userId, int fileId, EncodingProfile encodingProfile)
        {
            // Create output directory for current user, file, and encoding profile
            string currentDir = Directory.GetCurrentDirectory();
            var outputDir = Path.Combine(currentDir, "temp", $"{userId}", $"{fileId}", $"{encodingProfile.Id}");
            Directory.CreateDirectory(outputDir);

            // Create HLS and DASH output subdirectories
            string hlsDir = Path.Combine(outputDir, "hls");
            string dashDir = Path.Combine(outputDir, "dash");

            Directory.CreateDirectory(hlsDir);
            Directory.CreateDirectory(dashDir);

            // Determine FFmpeg command based on encoding profile type (DASH or HLS)
            string ffmpegArgs;
            if (encodingProfile.FormatType?.ToLower() == "dash")
            {
                ffmpegArgs = $"-y -i \"{filePath}\" {encodingProfile.FfmpegArgs} \"{dashDir}/manifest.mpd\"";
            }
            else if (encodingProfile.FormatType?.ToLower() == "hls")
            {
                ffmpegArgs = $"-y -i \"{filePath}\" {encodingProfile.FfmpegArgs} " +
                             $"-hls_segment_filename \"{hlsDir}/segment_%03d.m4s\" " +
                             $"\"{hlsDir}/playlist.m3u8\"";
            }
            else
            {
                // Unsupported profile type
                throw new InvalidOperationException($"Unsupported encoding profile type: {encodingProfile.FormatType}");
            }

            // Log the final FFmpeg command
            _logger.LogInformation("🎬 FFmpeg command for transcoding: {FFmpegArgs}", ffmpegArgs);

            try
            {
                // Execute FFmpeg process
                await RunFFmpegAsync(ffmpegArgs);

                // Upload output files to Azure Blob Storage
                return await _cloudStorageService.UploadTranscodedOutputAsync(outputDir, fileName, fileId, userId, encodingProfile.Id);
            }
            finally
            {
                // Cleanup temporary directory regardless of success/failure
                await _cleanerService.CleanDirectoryContentsAsync(outputDir);
                _logger.LogInformation("🧹 Cleaned up temporary directory: {OutputDir}", outputDir);
            }
        }



        private async Task RunFFmpegAsync(string args)
        {
            // Prepare the FFmpeg process with the given arguments
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = args,
                    RedirectStandardError = true, // Capture error output from FFmpeg
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            // Start the FFmpeg process
            process.Start();

            // Read any error output
            string stderr = await process.StandardError.ReadToEndAsync();

            // Wait for the process to complete
            await process.WaitForExitAsync();

            // Log success message
            _logger.LogInformation("🎬 FFmpeg transcoding completed successfully.");

            // Throw an exception if FFmpeg failed
            if (process.ExitCode != 0)
            {
                _logger.LogError("❌ FFmpeg exited with code {ExitCode}: {Error}", process.ExitCode, stderr);
                throw new Exception($"FFmpeg failed:\n{stderr}");
            }
        }


        public async Task<Stream> GenerateThumbnailFromDirAsync(string time, string fileName, int userId, int fileId)
        {
            // Validate time input
            if (string.IsNullOrEmpty(time))
                throw new ArgumentException("Time cannot be null or empty", nameof(time));

            var thumbnailStream = new MemoryStream();

            try
            {
                // Download video from Azure Blob to local path
                string inputPath = await _cloudStorageService.DownloadVideoToLocalAsync(fileName, userId, fileId);

                // Build FFmpeg command to capture one frame at the given timestamp and output it as a JPEG stream
                var ultraFastArgs = $"-ss {time} -i \"{inputPath}\" -vframes 1 -f image2pipe -vcodec mjpeg -q:v 2 -an -s 320x180 pipe:1";

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ultraFastArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                _logger.LogInformation("📸 Starting FFmpeg process to generate thumbnail at time {Time} from file: {FileName}", time, fileName);

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    // Copy FFmpeg output (JPEG image) to the memory stream
                    await process.StandardOutput.BaseStream.CopyToAsync(thumbnailStream);

                    // Wait until FFmpeg process completes
                    await process.WaitForExitAsync();

                    // Handle non-zero exit code
                    if (process.ExitCode != 0)
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        _logger.LogError("❌ FFmpeg failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                        throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}: {error}");
                    }
                }

                _logger.LogInformation("✅ FFmpeg thumbnail generation completed successfully.");

                // Reset stream position to beginning before returning
                thumbnailStream.Position = 0;
                return thumbnailStream;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating thumbnail from video at time {Time}", time);

                // Ensure memory stream is cleaned up if something goes wrong
                thumbnailStream?.Dispose();
                throw;
            }
        }



        /// <summary>
        /// Generates multiple thumbnails from a video file and saves them locally.
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <param name="fileId">Video file identifier</param>
        /// <param name="filePath">Local path to the video file</param>
        /// <returns>Path to the directory containing generated thumbnails</returns>
        public async Task<string> GenerateMultipleThumbnailsAsync(int userId, int fileId, string filePath)
        {
            try
            {
                // Construct local thumbnail output directory
                string currentDir = Directory.GetCurrentDirectory();
                string thumbnailDir = Path.Combine(currentDir, "temp", $"{userId}", $"{fileId}", "thumbnails");
                Directory.CreateDirectory(thumbnailDir);

                // Ensure the input video file exists
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Input video file not found: {filePath}");

                // Prepare output file pattern for thumbnails (e.g., thumb_001.jpg, thumb_002.jpg, ...)
                string outputPattern = Path.Combine(thumbnailDir, "thumb_%03d.jpg");

                // FFmpeg arguments:
                // -vf fps=1/5: extract 1 frame every 5 seconds
                // -scale=320:180: resize to 320x180
                // -frames:v 5: extract 5 thumbnails
                string ffmpegArgs = $"-i \"{filePath}\" -vf fps=1/5,scale=320:180 -frames:v 5 \"{outputPattern}\"";

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegArgs,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = new Process { StartInfo = psi };

                // Start FFmpeg thumbnail extraction
                process.Start();

                // Capture both stdout and stderr
                var errorTask = process.StandardError.ReadToEndAsync();
                var outputTask = process.StandardOutput.ReadToEndAsync();

                // Wait for process to finish
                await process.WaitForExitAsync();

                // Read FFmpeg outputs
                var error = await errorTask;
                var output = await outputTask;

                // If FFmpeg exits with non-zero status, throw an exception
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}. Error: {error}");
                }

                // Verify that thumbnails were actually created
                var createdThumbnails = Directory.GetFiles(thumbnailDir, "thumb_*.jpg");
                if (createdThumbnails.Length == 0)
                {
                    throw new InvalidOperationException("No thumbnails were generated");
                }

                _logger.LogInformation("✅ Generated {Count} thumbnails in: {Directory}", createdThumbnails.Length, thumbnailDir);

                return thumbnailDir;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating thumbnails");
                throw;
            }
        }
    }

}