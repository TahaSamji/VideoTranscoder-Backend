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
                var defaultProfile = new EncodingProfile
                {
                    Name = "Default H.264 720p",
                    FfmpegArgs = "-c:v libx264 -preset fast -crf 23 -c:a aac -b:a 128k",
                    Resolution = "1280x720",
                    Bitrate = "1500k",
                    CreatedAt = DateTime.UtcNow,
                    FormatType = "hls"
                
                };

                context.EncodingProfiles.Add(defaultProfile);
            }
                await context.SaveChangesAsync();
            }
        }
    }
}
