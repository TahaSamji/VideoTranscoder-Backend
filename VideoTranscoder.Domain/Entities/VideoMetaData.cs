using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
    public class VideoMetaData
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public required string OriginalFilename { get; set; }
        public required string BlobPath { get; set; }
        public  required string Status { get; set; }
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        public List<Thumbnail> Thumbnails { get; set; }
    }
}