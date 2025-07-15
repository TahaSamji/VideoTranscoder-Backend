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

            modelBuilder.Entity<VideoVariant>()
           .HasOne(v => v.TranscodingJob)
           .WithMany()
           .HasForeignKey(v => v.TranscodingJobId)
           .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<TranscodingJob>()
                .HasOne<VideoMetaData>()
                .WithMany()
                .HasForeignKey(j => j.VideoFileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VideoVariant>()
                         .HasOne<VideoMetaData>()
                         .WithMany()
                         .HasForeignKey(v => v.VideoFileId)
                         .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TranscodingJob>()
                .HasOne<EncodingProfile>()
                .WithMany()
                .HasForeignKey(j => j.EncodingProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TranscodingJob>()
        .HasIndex(j => new { j.VideoFileId, j.EncodingProfileId })
        .HasDatabaseName("IX_TranscodingJob_VideoFile_EncodingProfile");

        }
    }
}
