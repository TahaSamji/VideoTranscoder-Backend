using System;
using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
    public class VideoVariant
    {
        [Key]
        public int Id { get; set; }
        public int TranscodingJobId { get; set; }
        public string Type { get; set; }
        public string BlobPath { get; set; }
        public string Resolution { get; set; }
        public string BitrateKbps { get; set; }
        public long Size { get; set; }
        public int DurationSeconds { get; set; }
        public DateTime CreatedAt { get; set; }
        public string VideoURL { get; set; }
    }
}
