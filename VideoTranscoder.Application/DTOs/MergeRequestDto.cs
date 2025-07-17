using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class MergeRequestDto
{
    [JsonPropertyName("totalChunks")]
    [Range(1, int.MaxValue, ErrorMessage = "TotalChunks must be at least 1.")] // Must have at least 1 chunk
    public int TotalChunks { get; set; }

    [JsonPropertyName("outputFileName")]
    [Required(ErrorMessage = "OutputFileName is required.")]
    [StringLength(255, ErrorMessage = "OutputFileName cannot exceed 255 characters.")] // Limit filename length
    public string OutputFileName { get; set; }

    [JsonPropertyName("fileSize")]
    [Range(1, long.MaxValue, ErrorMessage = "FileSize must be greater than 0.")] // File size must be positive
    public long FileSize { get; set; }

    [JsonPropertyName("width")]
    [Range(1, int.MaxValue, ErrorMessage = "Width must be a positive integer.")]
    public required int Width { get; set; }

    [JsonPropertyName("height")]
    [Range(1, int.MaxValue, ErrorMessage = "Height must be a positive integer.")]
    public required int Height { get; set; }

    [JsonPropertyName("duration")]
    [Range(1, int.MaxValue, ErrorMessage = "Duration must be greater than 0 seconds.")]
    public int Duration { get; set; }

    [JsonPropertyName("resolution")]
    [Required(ErrorMessage = "Resolution is required.")]
    [RegularExpression(@"^\d+x\d+$", ErrorMessage = "Resolution must be in the format WIDTHxHEIGHT (e.g., 1920x1080).")]
    public required string Resolution { get; set; }

    [JsonPropertyName("mimeType")]
    [Required(ErrorMessage = "MIMEType is required.")]
    [RegularExpression(@"^video\/[a-zA-Z0-9\.\-\+]+$", ErrorMessage = "Invalid MIME type format.")]
    public required string MIMEType { get; set; }

    [JsonPropertyName("browserType")]
    [Required(ErrorMessage = "BrowserType is required.")]
    public required string BrowserType { get; set; }
}
