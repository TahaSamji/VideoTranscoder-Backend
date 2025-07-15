using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using VideoTranscoder.VideoTranscoder.Application.enums;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Configurations
{
    public static class DataInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Apply any pending migrations
            await context.Database.MigrateAsync();

            // Check if any users exist
            if (!context.Users.Any())
            {
                var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("pass1234"));

                var user = new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    CreatedAt = DateTime.UtcNow,
                    PasswordHash = Convert.ToBase64String(bytes),
                    Role = UserRole.Admin
                    // Password should be hashed in real apps
                    // Here just saving a placeholder for demo
                };

                context.Users.Add(user);

                // 🎞️ Seed default encoding profile if not exists
                if (!context.EncodingProfiles.Any())
                {
                    var encoding1 = new EncodingProfile
                    {
                        Name = "H.265 2160p (4K)",
                        FfmpegArgs = "-c:v libx265 -preset slow -crf 24 -x265-params log-level=error -c:a aac -b:a 128k",
                        Resolution = "3840x2160",
                        Bitrate = "18000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls"
                    };

                    var encoding2 = new EncodingProfile
                    {
                        Name = "H.264 1440p (2K)",
                        FfmpegArgs = "-c:v libx264 -preset fast -crf 22 -profile:v high -level 4.2 -c:a aac -b:a 128k",
                        Resolution = "2560x1440",
                        Bitrate = "8000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls"
                    };

                    var encoding3 = new EncodingProfile
                    {
                        Name = "H.264 1080p",
                        FfmpegArgs = "-c:v libx264 -preset fast -crf 23 -profile:v high -level 4.1 -c:a aac -b:a 128k",
                        Resolution = "1920x1080",
                        Bitrate = "5000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls"
                    };

                    var encoding4 = new EncodingProfile
                    {
                        Name = "H.264 720p",
                        FfmpegArgs = "-c:v libx264 -preset fast -crf 23 -profile:v main -level 3.1 -c:a aac -b:a 128k",
                        Resolution = "1280x720",
                        Bitrate = "3000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls"
                    };

                    var encoding5 = new EncodingProfile
                    {
                        Name = "H.264 540p",
                        FfmpegArgs = "-c:v libx264 -preset fast -crf 24 -profile:v main -level 3.0 -c:a aac -b:a 96k",
                        Resolution = "960x540",
                        Bitrate = "2200k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls"
                    };

                    var encoding6 = new EncodingProfile
                    {
                        Name = "H.264 480p",
                        FfmpegArgs = "-c:v libx264 -preset veryfast -crf 24 -profile:v main -level 3.0 -c:a aac -b:a 96k",
                        Resolution = "854x480",
                        Bitrate = "1500k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls"
                    };

                    var encoding7 = new EncodingProfile
                    {
                        Name = "H.264 360p",
                        FfmpegArgs = "-c:v libx264 -preset veryfast -crf 25 -profile:v baseline -level 3.0 -c:a aac -b:a 64k",
                        Resolution = "640x360",
                        Bitrate = "800k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls"
                    };

                    var encoding8 = new EncodingProfile
                    {
                        Name = "H.264 240p",
                        FfmpegArgs = "-c:v libx264 -preset veryfast -crf 26 -profile:v baseline -level 3.0 -c:a aac -b:a 48k",
                        Resolution = "426x240",
                        Bitrate = "400k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls"
                    };


                    context.EncodingProfiles.AddRange(
     encoding1, encoding2, encoding3, encoding4,
     encoding5, encoding6, encoding7, encoding8
 );
                }
                await context.SaveChangesAsync();
            }
        }
    }
}
