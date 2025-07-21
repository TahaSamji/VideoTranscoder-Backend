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

                };

                context.Users.Add(user);

                // 🎞️ Seed default encoding profile if not exists
                if (!context.EncodingProfiles.Any(p => p.IsActive))
                {
                    var encoding1 = new EncodingProfile
                    {
                        Name = "H.265 2160p (4K)",
                        FfmpegArgs = "-c:v libx265 -s 3840x2160 -b:v 18000k -crf 24 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 128k -f hls -hls_time 4 -hls_segment_type fmp4 -hls_playlist_type vod",
                        Resolution = "3840x2160",
                        Width = 3840,
                        Height = 2160,
                        Bitrate = "18000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls",
                        BrowserType = "Edge"
                    };

                    var encoding2 = new EncodingProfile
                    {
                        Name = "H.264 1440p (2K)",
                        FfmpegArgs = "-c:v libx264 -s 2560x1440 -b:v 8000k -crf 22 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 128k -f hls -hls_time 4 -hls_segment_type fmp4 -hls_playlist_type vod",
                        Resolution = "2560x1440",
                        Width = 2560,
                        Height = 1440,
                        Bitrate = "8000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls",
                        BrowserType = "Edge",
                        IsAdminSelected = true

                    };

                    var encoding3 = new EncodingProfile
                    {
                        Name = "H.264 1080p",
                        FfmpegArgs = "-c:v libx264 -s 1920x1080 -b:v 5000k -crf 23 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 128k -f hls -hls_time 4 -hls_segment_type fmp4 -hls_playlist_type vod",
                        Resolution = "1920x1080",
                        Width = 1920,
                        Height = 1080,
                        Bitrate = "5000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls",
                        BrowserType = "Edge",
                        IsAdminSelected = true

                    };

                    var encoding4 = new EncodingProfile
                    {
                        Name = "H.264 720p",
                        FfmpegArgs = "-c:v libx264 -s 1280x720 -b:v 3000k -crf 23 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 128k -f hls -hls_time 4 -hls_segment_type fmp4 -hls_playlist_type vod",
                        Resolution = "1280x720",
                        Width = 1280,
                        Height = 720,
                        Bitrate = "3000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls",
                        BrowserType = "Edge"
                    };

                    var encoding5 = new EncodingProfile
                    {
                        Name = "H.264 540p",
                        FfmpegArgs = "-c:v libx264 -s 960x540 -b:v 2200k -crf 24 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 96k -f hls -hls_time 4 -hls_segment_type fmp4 -hls_playlist_type vod",
                        Resolution = "960x540",
                        Width = 960,
                        Height = 540,
                        Bitrate = "2200k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls",
                        BrowserType = "Edge",
                        IsAdminSelected = true

                    };

                    var encoding6 = new EncodingProfile
                    {
                        Name = "H.264 480p",
                        FfmpegArgs = "-c:v libx264 -s 854x480 -b:v 1500k -crf 24 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 96k -f hls -hls_time 4 -hls_segment_type fmp4 -hls_playlist_type vod",
                        Resolution = "854x480",
                        Width = 854,
                        Height = 480,
                        Bitrate = "1500k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls",
                        BrowserType = "Edge"
                    };

                    var encoding7 = new EncodingProfile
                    {
                        Name = "H.264 360p",
                        FfmpegArgs = "-c:v libx264 -s 640x360 -b:v 800k -crf 25 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 64k -f hls -hls_time 4 -hls_segment_type fmp4 -hls_playlist_type vod",
                        Resolution = "640x360",
                        Width = 640,
                        Height = 360,
                        Bitrate = "800k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls",
                        BrowserType = "Edge",
                                                IsAdminSelected = true

                    };

                    var encoding8 = new EncodingProfile
                    {
                        Name = "H.264 240p",
                        FfmpegArgs = "-c:v libx264 -s 426x240 -b:v 400k -crf 26 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 48k -f hls -hls_time 4 -hls_segment_type fmp4 -hls_playlist_type vod",
                        Resolution = "426x240",
                        Width = 426,
                        Height = 240,
                        Bitrate = "400k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "hls",
                        BrowserType = "Edge",
                        IsAdminSelected = true

                    };

                    var dash1 = new EncodingProfile
                    {
                        Name = "H.264 2160p (4K)",
                        FfmpegArgs = "-c:v libx264 -s 3840x2160 -b:v 18000k -crf 23 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 128k -f dash -seg_duration 4 -use_template 1 -use_timeline 1 -init_seg_name init-$RepresentationID$.mp4 -media_seg_name chunk-$RepresentationID$-$Number$.m4s -adaptation_sets \"id=0,streams=v id=1,streams=a\"",
                        Resolution = "3840x2160",
                        Width = 3840,
                        Height = 2160,
                        Bitrate = "18000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "dash",
                        BrowserType = "Chrome",
                        IsAdminSelected = true

                    };

                    var dash2 = new EncodingProfile
                    {
                        Name = "H.264 1080p",
                        FfmpegArgs = "-c:v libx264 -s 1920x1080 -b:v 5000k -crf 23 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 128k -f dash -seg_duration 4 -use_template 1 -use_timeline 1 -init_seg_name init-$RepresentationID$.mp4 -media_seg_name chunk-$RepresentationID$-$Number$.m4s -adaptation_sets \"id=0,streams=v id=1,streams=a\"",
                        Resolution = "1920x1080",
                        Width = 1920,
                        Height = 1080,
                        Bitrate = "5000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "dash",
                        BrowserType = "Chrome",
                        
                    };

                    var dash3 = new EncodingProfile
                    {
                        Name = "H.265 720p",
                        FfmpegArgs = "-c:v libx265 -s 1280x720 -b:v 3000k -crf 23 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 128k -f dash -seg_duration 4 -use_template 1 -use_timeline 1 -init_seg_name init-$RepresentationID$.mp4 -media_seg_name chunk-$RepresentationID$-$Number$.m4s -adaptation_sets \"id=0,streams=v id=1,streams=a\"",
                        Resolution = "1280x720",
                        Width = 1280,
                        Height = 720,
                        Bitrate = "3000k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "dash",
                        BrowserType = "Safari",
                        IsAdminSelected = true

                    };

                    var dash4 = new EncodingProfile
                    {
                        Name = "H.264 480p",
                        FfmpegArgs = "-c:v libx264 -s 854x480 -b:v 1500k -crf 23 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 128k -f dash -seg_duration 4 -use_template 1 -use_timeline 1 -init_seg_name init-$RepresentationID$.mp4 -media_seg_name chunk-$RepresentationID$-$Number$.m4s -adaptation_sets \"id=0,streams=v id=1,streams=a\"",
                        Resolution = "854x480",
                        Width = 854,
                        Height = 480,
                        Bitrate = "1500k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "dash",
                        BrowserType = "Chrome",
                        IsAdminSelected = true
                    };

                    var dash5 = new EncodingProfile
                    {
                        Name = "H.264 360p",
                        FfmpegArgs = "-c:v libx264 -s 640x360 -b:v 800k -crf 23 -preset medium -r 30 -g 120 -keyint_min 120 -sc_threshold 0 -c:a aac -b:a 128k -f dash -seg_duration 4 -use_template 1 -use_timeline 1 -init_seg_name init-$RepresentationID$.mp4 -media_seg_name chunk-$RepresentationID$-$Number$.m4s -adaptation_sets \"id=0,streams=v id=1,streams=a\"",
                        Resolution = "640x360",
                        Width = 640,
                        Height = 360,
                        Bitrate = "800k",
                        CreatedAt = DateTime.UtcNow,
                        FormatType = "dash",
                        BrowserType = "Chrome"
                    };


                    context.EncodingProfiles.AddRange(
                        encoding1, encoding2, encoding3, encoding4,
                        encoding5, encoding6, encoding7, encoding8,
                        dash1, dash2, dash3, dash4, dash5
                    );

                }

                await context.SaveChangesAsync();


            }
        }
    }
}
