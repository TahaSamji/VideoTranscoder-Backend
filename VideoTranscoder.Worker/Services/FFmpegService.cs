using System.Diagnostics;
using System.Runtime.CompilerServices;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    public class FFmpegService
    {
        private readonly ILogger<FFmpegService> _logger;
        private readonly ICloudStorageService _cloudStorageService;
         private readonly IEncryptionService _encyptionService;


        public FFmpegService(ILogger<FFmpegService> logger,IEncryptionService encyptionService, ICloudStorageService cloudStorageService)
        {
            _logger = logger;
            _cloudStorageService = cloudStorageService;
            _encyptionService = encyptionService;
        }

        public async Task<string> TranscodeToCMAFAsync(string filename, int userId, int fileId, EncodingProfile encodingProfile)
        {
            // Ensure output directory exists
            string currentDir = Directory.GetCurrentDirectory();
            var outputDir = Path.Combine(currentDir, "temp", $"{userId}", $"{fileId}", $"{encodingProfile.Id}", "output");
            Directory.CreateDirectory(outputDir);

            // Create subdirectories for HLS and DASH
            string hlsDir = Path.Combine(outputDir, "hls");
            string dashDir = Path.Combine(outputDir, "dash");

            Directory.CreateDirectory(hlsDir);
            Directory.CreateDirectory(dashDir);

            // string inputPath = await _cloudStorageService.DownloadVideoToLocalAsync(filename, userId, fileId);
            string inputPath = Path.Combine(currentDir, "input", $"{userId}", $"{fileId}", "videos", filename);

            // Build FFmpeg command based on profile type
            string ffmpegArgs;
            if (encodingProfile.FormatType?.ToLower() == "dash")
            {
                ffmpegArgs = $"-y -i \"{inputPath}\" {encodingProfile.FfmpegArgs} \"{dashDir}/manifest.mpd\"";
            }
            else if (encodingProfile.FormatType?.ToLower() == "hls")
            {
                ffmpegArgs = $"-y -i \"{inputPath}\" {encodingProfile.FfmpegArgs} " +
                             $"-hls_segment_filename \"{hlsDir}/segment_%03d.m4s\" " +
                             $"\"{hlsDir}/playlist.m3u8\"";
            }
            else
            {
                throw new InvalidOperationException($"Unsupported encoding profile type: {encodingProfile.FormatType}");
            }

            Console.WriteLine("🎬 FFmpeg Args: " + ffmpegArgs);

            await RunFFmpegAsync(ffmpegArgs);
            return await _cloudStorageService.UploadTranscodedOutputAsync(outputDir, filename, fileId, userId, encodingProfile.Id);
        }

        // public async Task<string> TranscodeToCMAFWithCENCAsync(string filename, int userId, int fileId, EncodingProfile encodingProfile)
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
        //     // string inputPath = Path.Combine(currentDir, "input", $"{userId}", $"{fileId}", "videos", filename);
        //     string inputPath = Path.Combine(currentDir, "input", "1", "1", "videos", "newvid.mp4");

        //     // Build FFmpeg command based on profile type
        //     // string ffmpegArgs;
        //     // if (encodingProfile.FormatType?.ToLower() == "dash")
        //     // {
        //     //     ffmpegArgs = $"-y -i \"{inputPath}\" {encodingProfile.FfmpegArgs} \"{dashDir}/manifest.mpd\"";
        //     // }
        //     // else if (encodingProfile.FormatType?.ToLower() == "hls")
        //     // {
        //     //     ffmpegArgs = $"-y -i \"{inputPath}\" {encodingProfile.FfmpegArgs} " +
        //     //                  $"-hls_segment_filename \"{hlsDir}/segment_%03d.m4s\" " +
        //     //                  $"\"{hlsDir}/playlist.m3u8\"";
        //     // }
        //     // else
        //     // {
        //     //     throw new InvalidOperationException($"Unsupported encoding profile type: {encodingProfile.FormatType}");
        //     // }
        //     // string ffmpegArgs = $"-y -i \"{inputPath}\" " +
        //     //         "-c:v libx264 -b:v 3000k -s 854x480 -r 30 -preset medium -crf 23 -g 60 -keyint_min 60 -sc_threshold 0 " +
        //     //         "-c:a aac -b:a 128k " +
        //     //         "-movflags +faststart " +
        //     //         $"\"{dashDir}/output_encoded.mp4\"";

        //     // Console.WriteLine("🎬 FFmpeg Args: " + ffmpegArgs);

        //     // await RunFFmpegAsync(ffmpegArgs);
        //     await _encyptionService.EncryptToHLSWithCENCAsync(dashDir,userId,fileId,encodingProfile);
        //     _logger.LogInformation("Dome witn encytption"); 
        //     // return await _cloudStorageService.UploadTranscodedOutputAsync(outputDir, filename, fileId, userId, encodingProfile.Id);
        //     return "done";
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
                Console.WriteLine("FFmpeg transcoding Started.");
                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    // Copy FFmpeg output (PNG image) to memory stream
                    await process.StandardOutput.BaseStream.CopyToAsync(thumbnailStream);

                    // Wait for process to complete
                    await process.WaitForExitAsync();
                    Console.WriteLine("FFmpeg Thumbnail Generation completed successfully.");
                    if (process.ExitCode != 0)
                    {
                        var error = await process.StandardError.ReadToEndAsync();
                        throw new InvalidOperationException($"FFmpeg failed with exit code {process.ExitCode}: {error}");
                    }
                }

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
        public async Task<string> GenerateMultipleThumbnailsAsync(string fileName, int userId, int fileId)
        {
            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                string thumbnailDir = Path.Combine(currentDir, "temp", $"{userId}", $"{fileId}", "thumbnails");
                Directory.CreateDirectory(thumbnailDir);

                // 1. Download video to local path
                string inputPath = Path.Combine(currentDir, "input", $"{userId}", $"{fileId}", "videos", fileName);

                // Check if input file exists
                if (!File.Exists(inputPath))
                    throw new FileNotFoundException($"Input video file not found: {inputPath}");

                // 2. Generate thumbnails with proper output pattern
                string outputPattern = Path.Combine(thumbnailDir, "thumb_%03d.jpg");
                string ffmpegArgs = $"-i \"{inputPath}\" -vf fps=1/10,scale=320:180 -frames:v 5 \"{outputPattern}\"";

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