using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
    public class VideoMetaData
    {
        // Primary key
        [Key]
        public int Id { get; set; }

        // ID of the user who uploaded the video
        [Required]
        public required int UserId { get; set; }

        // Original filename of the uploaded video
        [Required]
        public required string OriginalFilename { get; set; }

        // Video resolution in the format 1920x1080, etc.
        [Required]
        public required string Resolution { get; set; }

        // Width of the video in pixels
        [Required]
        public required int Width { get; set; }

        // Height of the video in pixels
        [Required]
        public required int Height { get; set; }

        // Total number of uploaded chunks for this video
        [Required]
        public required int TotalChunks { get; set; }

        // Duration of the video in seconds
        [Required]
        public required int Duration { get; set; }

        // MIME type of the video (e.g., video/mp4)
        [Required]
        public required string MIMEType { get; set; }

        // Path of the video blob in Azure Blob Storage
        [Required]
        public required string BlobPath { get; set; }

        // Status of the video (e.g., Uploaded, Processing, Completed)
        [Required]
        public required string Status { get; set; }

        // Size of the video file in bytes
        [Required]
        public required long Size { get; set; }

        // Timestamp when the video metadata was created
        public DateTime CreatedAt { get; set; }

        // Timestamp when the video metadata was last updated
        public DateTime UpdatedAt { get; set; }

        // Default thumbnail URL (optional, defaults to empty string if not set)
        public string defaultThumbnailUrl { get; set; } = string.Empty;
    }
}
