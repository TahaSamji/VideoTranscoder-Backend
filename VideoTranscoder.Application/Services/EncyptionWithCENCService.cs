using System.Diagnostics;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

public class CencEncryptionService : IEncryptionService
{
    public async Task<string> EncryptWithCENCAsync(string input, int userId, int fileId, EncodingProfile profile)
    {
        string currentDir = Directory.GetCurrentDirectory();
        var inputDir = Path.Combine(currentDir, "temp", "1", "1", "2", "output", "dash");

        string outputDir = Path.Combine(inputDir, "encrypted");
        Directory.CreateDirectory(outputDir);

        string inputFile = Path.Combine(inputDir, "output_encoded.mp4");

        var arguments = $"input=\"{inputFile}\",stream=video,output=\"{outputDir}/video_sd.m4s\",drm_label=SD " +
                        $"--enable_raw_key_encryption " +
                        $"--keys label=SD:key_id=abba271e8bcf552bbd2e86a434a9a5d9:key=69eaa802a6763af979e8d1940fb88392 " +
                        $"--protection_scheme cenc " +
                        $"--clear_lead 0 " +
                        $"--mpd_output \"{outputDir}/manifest.mpd\"";

        await RunShakaPackager(arguments);
        return outputDir;
    }

    public async Task<string> EncryptToHLSWithCENCAsync(string input, int userId, int fileId, EncodingProfile profile)
    {
        string currentDir = Directory.GetCurrentDirectory();
        var inputDir = Path.Combine(currentDir, "temp", "1", "1", "2", "output", "dash");

        string outputDir = Path.Combine(inputDir, "hls-encrypted");
        Directory.CreateDirectory(outputDir);

        string inputFile = Path.Combine(inputDir, "output_encoded.mp4");

        var arguments = $"input=\"{inputFile}\",stream=video,output=\"{outputDir}/video_sd.m4s\",drm_label=SD " +
                        $"--enable_raw_key_encryption " +
                        $"--keys label=SD:key_id=abba271e8bcf552bbd2e86a434a9a5d9:key=69eaa802a6763af979e8d1940fb88392 " +
                        $"--protection_scheme cenc " +
                        $"--clear_lead 0 " +
                        $"--hls_master_playlist_output \"{outputDir}/master.m3u8\"";

        await RunShakaPackager(arguments);
        return outputDir;
    }

    private async Task RunShakaPackager(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "packager",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        Console.WriteLine($"🔧 Executing command: packager {arguments}");

        process.Start();

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();

        Console.WriteLine("📤 Shaka STDOUT:\n" + stdout);
        Console.WriteLine("🛠️ Shaka STDERR:\n" + stderr);

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Shaka Packager failed with exit code {process.ExitCode}. Error: {stderr}");
        }
    }
}
