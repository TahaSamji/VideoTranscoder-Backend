using System;
using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
    public class TranscodingJob
    {
        [Key]
        public int Id { get; set; }
        public int VideoFileId { get; set; }
        public int EncodingProfileId { get; set; }
        public required string Status { get; set; }
        public required string BlobPath { get; set; }
        public int Progress { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}