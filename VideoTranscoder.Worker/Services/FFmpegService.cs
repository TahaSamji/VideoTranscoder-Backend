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
            // Ensure output directory exists
            string currentDir = Directory.GetCurrentDirectory();
            var outputDir = Path.Combine(currentDir, "temp", $"{userId}", $"{fileId}", $"{encodingProfile.Id}");
            Directory.CreateDirectory(outputDir);

            // Create subdirectories for HLS and DASH
            string hlsDir = Path.Combine(outputDir, "hls");
            string dashDir = Path.Combine(outputDir, "dash");

            Directory.CreateDirectory(hlsDir);
            Directory.CreateDirectory(dashDir);


            // Build FFmpeg command based on profile type
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
                throw new InvalidOperationException($"Unsupported encoding profile type: {encodingProfile.FormatType}");
            }

            Console.WriteLine("🎬 FFmpeg Args: " + ffmpegArgs);

            try
            {
                await RunFFmpegAsync(ffmpegArgs);
                return await _cloudStorageService.UploadTranscodedOutputAsync(outputDir, fileName, fileId, userId, encodingProfile.Id);

            }
            finally
            {


                await _cleanerService.CleanDirectoryContentsAsync(outputDir);


            }
        }

        // public async Task<string> TranscodeToCMAFAsync(string filename, int userId, int fileId, EncodingProfile encodingProfile)
        // {
        //     // Ensure output directory exists
        //     string currentDir = Directory.GetCurrentDirectory();
        //     var outputDir = Path.Combine(currentDir, "temp", $"{userId}", $"{fileId}", $"{encodingProfile.Id}", "output");
        //     Directory.CreateDirectory(outputDir);

        //     // Create subdirectories for HLS and DASH
        //     string hlsDir = Path.Combine(outputDir, "hls");
        //     string dashDir = Path.Combine(outputDir, "dash");

        //     Directory.CreateDirectory(hlsDir);
        //     Directory.CreateDirectory(dashDir);

        //     // string inputPath = await _cloudStorageService.DownloadVideoToLocalAsync(filename, userId, fileId);
        //     string inputPath = Path.Combine(currentDir, "input", $"{userId}", $"{fileId}", "videos", filename);
        //     var status = _fileLockService.GetStatus(inputPath);
        //     if (status != FileStatus.Ready)
        //     {
        //         Console.WriteLine($"⏳ File '{inputPath}' status is '{status}', waiting...");
        //         var resultStatus = await _fileLockService.WaitUntilReadyAsync(inputPath);

        //         if (resultStatus != FileStatus.Ready)
        //         {
        //             Console.WriteLine("🚫 File did not reach ready state after waiting.");
        //             throw new InvalidOperationException("File not ready for transcoding.");
        //         }
        //     }

        //     // ✅ Proceed only if file exists and is marked ready
        //     if (!File.Exists(inputPath))
        //     {
        //         Console.WriteLine($"❌ File does not exist at expected path: {inputPath}");
        //         throw new FileNotFoundException("Input file not found.", inputPath);
        //     }

        //     // Build FFmpeg command based on profile type
        //     string ffmpegArgs;
        //     if (encodingProfile.FormatType?.ToLower() == "dash")
        //     {
        //         ffmpegArgs = $"-y -i \"{inputPath}\" {encodingProfile.FfmpegArgs} \"{dashDir}/manifest.mpd\"";
        //     }
        //     else if (encodingProfile.FormatType?.ToLower() == "hls")
        //     {
        //         ffmpegArgs = $"-y -i \"{inputPath}\" {encodingProfile.FfmpegArgs} " +
        //                      $"-hls_segment_filename \"{hlsDir}/segment_%03d.m4s\" " +
        //                      $"\"{hlsDir}/playlist.m3u8\"";
        //     }
        //     else
        //     {
        //         throw new InvalidOperationException($"Unsupported encoding profile type: {encodingProfile.FormatType}");
        //     }

        //     Console.WriteLine("🎬 FFmpeg Args: " + ffmpegArgs);

        //     await RunFFmpegAsync(ffmpegArgs);
        //     // FileUsageTracker.Decrement(inputPath);
        //     _fileLockService.MarkReady(inputPath);
        //     return await _cloudStorageService.UploadTranscodedOutputAsync(outputDir, filename, fileId, userId, encodingProfile.Id);
        // }



        private async Task RunFFmpegAsync(string args)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = args,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            Console.WriteLine("FFmpeg transcoding completed successfully.");
            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg failed:\n{stderr}");
            }
        }

        public async Task<Stream> GenerateThumbnailFromDirAsync(string time, string fileName, int userId, int fileId)
        {

            if (string.IsNullOrEmpty(time))
                throw new ArgumentException("Time cannot be null or empty", nameof(time));
            var thumbnailStream = new MemoryStream();
            try
            {
                // Create a memory stream to capture the thumbnail

                // string currentDir = Directory.GetCurrentDirectory();
                // var outputDir = Path.Combine(currentDir, "temp", $"{userId}", $"{fileId}", "thumbnails");
                // Directory.CreateDirectory(outputDir);

                string inputPath = await _cloudStorageService.DownloadVideoToLocalAsync(fileName, userId, fileId);
                // here
                // FFmpeg command to generate thumbnail at specific time
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
                Console.WriteLine("FFmpeg Thumbnail Generation Started.");
                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    // Copy FFmpeg output to memory stream
                    await process.StandardOutput.BaseStream.CopyToAsync(thumbnailStream);

                    // Wait for process to complete
                    await process.WaitForExitAsync();

                    if (process.ExitCode != 0)
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}: {error}");
                    }

                }
                Console.WriteLine("FFmpeg Thumbnail Generation completed successfully.");

                // Reset stream position for reading
                thumbnailStream.Position = 0;

                return thumbnailStream;
            }
            catch
            {
                // Clean up memory stream if there's an error
                thumbnailStream?.Dispose();
                throw;
            }
        }
        public async Task<string> GenerateMultipleThumbnailsAsync(int userId, int fileId, string filePath)
        {
            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                string thumbnailDir = Path.Combine(currentDir, "temp", $"{userId}", $"{fileId}", "thumbnails");
                Directory.CreateDirectory(thumbnailDir);


                // Check if input file exists
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"Input video file not found: {filePath}");

                // 2. Generate thumbnails with proper output pattern
                string outputPattern = Path.Combine(thumbnailDir, "thumb_%03d.jpg");
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
                process.Start();

                // Read both stdout and stderr to prevent deadlock
                var errorTask = process.StandardError.ReadToEndAsync();
                var outputTask = process.StandardOutput.ReadToEndAsync();

                await process.WaitForExitAsync();

                var error = await errorTask;
                var output = await outputTask;

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}. Error: {error}");
                }

                // Verify thumbnails were created
                var createdThumbnails = Directory.GetFiles(thumbnailDir, "thumb_*.jpg");
                if (createdThumbnails.Length == 0)
                {
                    throw new InvalidOperationException("No thumbnails were generated");
                }
                Console.WriteLine($"✅ Generated {createdThumbnails.Length} thumbnails in: {thumbnailDir}");

                return thumbnailDir;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error generating thumbnails: {ex.Message}");
                throw;
            }
        }




    }



}