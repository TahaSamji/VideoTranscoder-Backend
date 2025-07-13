namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    public class VideoRenditionDto
    {
        public int VariantId { get; set; }
        public string Type { get; set; }             // HLS / DASH
        public string Resolution { get; set; }
        public string BitrateKbps { get; set; }
        public long Size { get; set; }
        public int DurationSeconds { get; set; }
        public string VideoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}