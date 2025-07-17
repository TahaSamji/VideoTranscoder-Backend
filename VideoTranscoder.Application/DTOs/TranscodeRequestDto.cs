using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    // DTO used to send a message to the transcoding worker/service
    // Typically sent over a message queue (e.g., Azure Service Bus, Kafka) to trigger distributed transcoding
    public class TranscodeRequestMessage
    {
        [Required(ErrorMessage = "FileId is required.")]
        public required int FileId { get; set; } // ID of the uploaded video file to be transcoded (usually corresponds to VideoMetaData.Id)

        [Required(ErrorMessage = "LocalVideoPath is required.")]
        public required string LocalVideoPath { get; set; } // Absolute or relative path to the merged video file on disk (used by FFmpeg)

        [Required(ErrorMessage = "EncodingProfileId is required.")]
        public required int EncodingProfileId { get; set; } // The encoding profile to apply (bitrate, resolution, format, etc.)

        [Required(ErrorMessage = "TotalRenditions is required.")]
        public required int TotalRenditions { get; set; } // How many variants (renditions) are expected to be generated from this encoding job
    }
}
