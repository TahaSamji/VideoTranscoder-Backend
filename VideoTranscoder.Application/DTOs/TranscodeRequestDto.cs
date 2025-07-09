namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    public class TranscodeRequestMessage
    {
        public required int FileId { get; set; }
        public required string BlobPath { get; set; }
        public required int EncodingProfileId { get; set; }
    }
}