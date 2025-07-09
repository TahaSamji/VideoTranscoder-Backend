namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{

    public class Thumbnail
    {
        public int Id { get; set; }
        public required int FileId { get; set; }
        public required string BlobUrl { get; set; }
        public required string TimeOffset { get; set; }
        public  required bool IsDefault { get; set; }
        public  required DateTime CreatedAt { get; set; }

    }
}