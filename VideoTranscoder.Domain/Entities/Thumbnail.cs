using System;
using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
    public class Thumbnail
    {
        // Primary key for the thumbnail
        [Key]
        public int Id { get; set; }

        // Associated file ID (foreign key to VideoMetaData)
        [Required]
        public int FileId { get; set; }

        // Full Blob Storage URL to the thumbnail image
        [Required]
        public required string BlobUrl { get; set; }

        // Time offset from the start of the video when the thumbnail was captured (e.g., "00:00:05")
        [Required]
        public required string TimeOffset { get; set; }

        // Indicates whether this thumbnail is marked as the default for the video
        [Required]
        public required bool IsDefault { get; set; }

        // Timestamp of when the thumbnail was created
        [Required]
        public required DateTime CreatedAt { get; set; }
    }
}
