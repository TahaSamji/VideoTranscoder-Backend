using System;
using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
    public class TranscodingJob
    {
        // Primary key of the transcoding job
        [Key]
        public int Id { get; set; }

        // Foreign key: ID of the video file being transcoded
        [Required]
        public int VideoFileId { get; set; }

        // Foreign key: ID of the encoding profile used
        [Required]
        public int EncodingProfileId { get; set; }

        // Current status of the transcoding job (e.g., "Pending", "Processing", "Completed", "Failed")
        [Required]
        public required string Status { get; set; }

        // Optional error message in case of failure
        public string ErrorMessage { get; set; } = string.Empty;

        // Timestamp when the job was created
        [Required]
        public DateTime CreatedAt { get; set; }

        // Timestamp when transcoding started (nullable)
        public DateTime? StartedAt { get; set; }

        // Timestamp when transcoding completed (nullable)
        public DateTime? CompletedAt { get; set; }
    }
}
