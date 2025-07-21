
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
  public class EncodingProfile
  {
    [Key]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("ffmpeg_args")]
    public required string FfmpegArgs { get; set; }

    [JsonPropertyName("resolution")]
    public required string Resolution { get; set; }

    [JsonPropertyName("bitrate")]
    public required string Bitrate { get; set; }

    public int Width { get; set; }
    public int Height { get; set; }

    [JsonPropertyName("format_type")]
    public required string FormatType { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsAdminSelected { get; set; } = false;
     
      [JsonPropertyName("browser_type")]
     public required string BrowserType { get; set; }
  }
}