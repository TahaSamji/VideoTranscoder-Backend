using Microsoft.EntityFrameworkCore;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Domain.DatabaseContext
{
    // Application database context handling all entity configurations and relationships
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet for users table
        public DbSet<User> Users { get; set; }

        // DbSet for uploaded video metadata
        public DbSet<VideoMetaData> VideoMetaDatas { get; set; }

        // DbSet for encoding/transcoding profiles
        public DbSet<EncodingProfile> EncodingProfiles { get; set; }

        // DbSet for transcoding jobs (tracks progress, status, etc.)
        public DbSet<TranscodingJob> TranscodingJobs { get; set; }

        // DbSet for each video variant/rendition (e.g., different resolutions/bitrates)
        public DbSet<VideoVariant> VideoVariants { get; set; }

        // DbSet for thumbnail images extracted from videos
        public DbSet<Thumbnail> Thumbnails { get; set; }

        // Fluent API configuration for relationships and constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // A VideoMetaData entry is linked to a User (many-to-one)
            modelBuilder.Entity<VideoMetaData>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting user deletes videos

            // A VideoVariant belongs to one TranscodingJob
            modelBuilder.Entity<VideoVariant>()
                .HasOne(v => v.TranscodingJob)
                .WithMany()
                .HasForeignKey(v => v.TranscodingJobId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if variants exist

            // A TranscodingJob belongs to a VideoMetaData
            modelBuilder.Entity<TranscodingJob>()
                .HasOne<VideoMetaData>()
                .WithMany()
                .HasForeignKey(j => j.VideoFileId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting video deletes jobs

            // A VideoVariant also belongs to a VideoMetaData
            modelBuilder.Entity<VideoVariant>()
                .HasOne<VideoMetaData>()
                .WithMany()
                .HasForeignKey(v => v.VideoFileId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting video deletes variants

            // A TranscodingJob uses one EncodingProfile
            modelBuilder.Entity<TranscodingJob>()
                .HasOne<EncodingProfile>()
                .WithMany()
                .HasForeignKey(j => j.EncodingProfileId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deletion if job depends on profile

            // Unique index to prevent for faster query
            modelBuilder.Entity<TranscodingJob>()
                .HasIndex(j => new { j.VideoFileId, j.EncodingProfileId })
                .HasDatabaseName("IX_TranscodingJob_VideoFile_EncodingProfile");
        }
    }
}
