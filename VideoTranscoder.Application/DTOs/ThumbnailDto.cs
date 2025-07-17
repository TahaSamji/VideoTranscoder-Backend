using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    // DTO representing a thumbnail image associated with a video.
    // Typically used when selecting or displaying video thumbnails in the UI.
    public class ThumbnailDto
    {
        public int Id { get; set; } // Unique identifier of the thumbnail (e.g., from the database)

        [Required(ErrorMessage = "Blob URL is required.")]
        [Url(ErrorMessage = "Invalid URL format.")]
        public string BlobUrl { get; set; } = string.Empty; // Public or signed URL to access the thumbnail image in Blob Storage

        [Required(ErrorMessage = "Time offset is required.")]
        public string TimeOffset { get; set; } = string.Empty; // Time in the video (e.g., "00:00:05") where this thumbnail was generated

        public bool IsDefault { get; set; } = false; // Indicates if this thumbnail is selected as the default preview image for the video
    }
}
