namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    public class ThumbnailDto
    {
        public int Id { get; set; }
        public string BlobUrl { get; set; } = string.Empty;
        public string TimeOffset { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}