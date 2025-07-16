using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
    public class VideoMetaData
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string OriginalFilename { get; set; }
        public required string Resolution { get; set; }
        public required int Width { get; set; }  
        public required int Height { get; set; }
        public required int TotalChunks { get; set; }
        public required int Duration { get; set; }
        public required string MIMEType { get; set; }
        public required string BlobPath { get; set; }
        public required string Status { get; set; }
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string defaultThumbnailUrl { get; set; } = string.Empty;
    }
}