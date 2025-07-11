using Microsoft.EntityFrameworkCore;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Domain.DatabaseContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<VideoMetaData> VideoMetaDatas { get; set; }
        public DbSet<EncodingProfile> EncodingProfiles { get; set; }
        public DbSet<TranscodingJob> TranscodingJobs { get; set; }
        public DbSet<VideoVariant> VideoVariants { get; set; }
        public DbSet<Thumbnail> Thumbnails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VideoMetaData>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VideoMetaData>()
                .HasMany(v => v.Thumbnails)
                .WithOne()
                .HasForeignKey(t => t.FileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VideoMetaData>()
                .HasOne<EncodingProfile>()
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TranscodingJob>()
                .HasOne<VideoMetaData>()
                .WithMany()
                .HasForeignKey(j => j.VideoFileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TranscodingJob>()
                .HasOne<EncodingProfile>()
                .WithMany()
                .HasForeignKey(j => j.EncodingProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<VideoVariant>()
                .HasOne<TranscodingJob>()
                .WithMany()
                .HasForeignKey(v => v.TranscodingJobId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
