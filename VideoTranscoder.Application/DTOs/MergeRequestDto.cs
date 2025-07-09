
namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    public class MergeRequestDto
    {
        public int TotalChunks { get; set; }
        public required string OutputFileName { get; set; }
        public long FileSize { get; set; }
        public int EncodingId { get; set; }

    }
}