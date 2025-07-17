using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
    public class VideoVariant
    {
        // Primary key
        [Key]
        public int Id { get; set; }

        // Foreign key referencing the transcoding job
        [Required]
        public int TranscodingJobId { get; set; }

        // Foreign key referencing the original uploaded video file
        [Required]
        public int VideoFileId { get; set; }

        // Navigation property for the transcoding job
        [ForeignKey("TranscodingJobId")]
        public TranscodingJob TranscodingJob { get; set; }

        // Type of variant (e.g., HLS, DASH)
        [Required]
        public required string Type { get; set; }

        // Path of the transcoded variant in blob storage
        [Required]
        public required string BlobPath { get; set; }

        // Resolution of the variant (e.g., 1280x720)
        [Required]
        public required string Resolution { get; set; }

        // Bitrate in Kbps (stored as string to support units like "1200k")
        [Required]
        public required string BitrateKbps { get; set; }

        // Duration of the variant in seconds
        [Required]
        public int DurationSeconds { get; set; }

        // Timestamp when this variant was created
        public DateTime CreatedAt { get; set; }

        // Public or signed CDN-accessible URL for the video
        [Required]
        public required string VideoURL { get; set; }
    }
}
