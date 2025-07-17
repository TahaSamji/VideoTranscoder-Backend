using System;
using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    // DTO representing a transcoded video rendition (variant), typically displayed in the player or UI.
    public class VideoRenditionDto
    {
        [Required] // Required: Maps to the corresponding VideoVariant entity
        public int VariantId { get; set; } // Unique ID of the video variant

        [Required]
        [StringLength(10)] // e.g., "HLS", "DASH"
        public string Type { get; set; } = string.Empty; // Streaming format type

        [Required]
        [RegularExpression(@"^\d{3,5}x\d{3,5}$", ErrorMessage = "Resolution must be in the format WIDTHxHEIGHT")] // e.g., "1920x1080"
        public string Resolution { get; set; } = string.Empty; // Resolution of the rendition

        [Required]
        [RegularExpression(@"^\d+$", ErrorMessage = "Bitrate must be a numeric value")]
        public string BitrateKbps { get; set; } = string.Empty; // Bitrate in Kbps as a string for display purposes

        [Required]
        [Range(1, long.MaxValue)] // Must be greater than 0 bytes
        public long Size { get; set; } // File size in bytes

        [Required]
        [Range(1, int.MaxValue)] // Must be a positive duration
        public int DurationSeconds { get; set; } // Duration in seconds

        [Required]
        [Url] // Must be a valid URL
        public string VideoUrl { get; set; } = string.Empty; // Playback URL (e.g., via CDN)

        public DateTime CreatedAt { get; set; } // Optional: Timestamp of when the rendition was created
    }
}
