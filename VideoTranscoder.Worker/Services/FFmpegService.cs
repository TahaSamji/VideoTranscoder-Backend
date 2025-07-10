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

        public FFmpegService(ILogger<FFmpegService> logger, ICloudStorageService cloudStorageService)
        {
            _logger = logger;
            _cloudStorageService = cloudStorageService;
        }

        public async Task<Stream> GenerateThumbnailAsync(string sasUrl, string time)
        {
            if (string.IsNullOrEmpty(sasUrl))
                throw new ArgumentException("SAS URL cannot be null or empty", nameof(sasUrl));

            if (string.IsNullOrEmpty(time))
                throw new ArgumentException("Time cannot be null or empty", nameof(time));

            // Create a memory stream to capture the thumbnail
            var thumbnailStream = new MemoryStream();

            try
            {
                string currentDir = Directory.GetCurrentDirectory();
                 var outputDir = Path.Combine(currentDir, "");
                // here
                // FFmpeg command to generate thumbnail at specific time
                var ultraFastArgs = $"-ss {time} -i \"{sasUrl}\" -vframes 1 -f image2pipe -vcodec mjpeg -q:v 2 -an -vf scale=1280:720 pipe:1";
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ultraFastArgs,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();

                    // Copy FFmpeg output (PNG image) to memory stream
                    await process.StandardOutput.BaseStream.CopyToAsync(thumbnailStream);

                    // Wait for process to complete
                    await process.WaitForExitAsync();

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
        public async Task TranscodeToCMAFAsync(string filename,int userId,int fileId)
        {
            // Ensure output directory exists

            string currentDir = Directory.GetCurrentDirectory();
            var outputDir = Path.Combine(currentDir, "temp", $"{userId}",$"{fileId}","output");
            Directory.CreateDirectory(outputDir);
            // Create subdirectories for HLS and DASH
            string hlsDir = Path.Combine(outputDir, "hls");
            string dashDir = Path.Combine(outputDir, "dash");

            Directory.CreateDirectory(hlsDir);
            Directory.CreateDirectory(dashDir);
            string inputPath = await _cloudStorageService.DownloadVideoToLocalAsync(filename,userId,fileId);

            // Prepare output filenames
            // var hlsOutput = Path.Combine(outputDir, "playlist.m3u8");
            // var dashOutput = Path.Combine(outputDir, "manifest.mpd");

            // Optional: CMAF-compliant segment naming
            // string baseName = Path.Combine(outputDir, "stream");

            // FFmpeg command (HLS + DASH outputs from single fMP4 segment stream)
            string args = $"-y -i \"{inputPath}\" " +
  "-map 0:v -map 0:a? " +
  "-c:v libx264 -b:v 3000k -g 48 -keyint_min 48 -sc_threshold 0 " +
  "-c:a aac -b:a 128k " +

  // HLS (CMAF) output
  "-f hls " +
  "-hls_time 4 -hls_playlist_type vod " +
  "-hls_segment_type fmp4 " +
  $"-hls_segment_filename \"{hlsDir}/segment_%03d.m4s\" " +
  $"\"{hlsDir}/playlist.m3u8\" " +

  // DASH (CMAF) output
  "-map 0:v -map 0:a? " +
  "-c:v libx264 -b:v 3000k -g 48 -keyint_min 48 -sc_threshold 0 " +
  "-c:a aac -b:a 128k " +
  "-f dash " +
  "-seg_duration 4 " +
  "-use_template 1 -use_timeline 1 " +
  "-init_seg_name init-$RepresentationID$.mp4 " +
  "-media_seg_name chunk-$RepresentationID$-$Number$.m4s " +
  "-adaptation_sets \"id=0,streams=v id=1,streams=a\" " +
  $"\"{dashDir}/manifest.mpd\"";


            // NOTE: Add HLS CMAF support if needed (requires separate FFmpeg run, or use Shaka Packager for both at once)

            await RunFFmpegAsync(args);
            await _cloudStorageService.UploadTranscodedOutputAsync(outputDir,filename,fileId,userId);
        }

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
    }



}