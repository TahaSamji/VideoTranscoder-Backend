using System.Text.Json.Serialization;

public class MergeRequestDto
{
    [JsonPropertyName("totalChunks")]
    public int TotalChunks { get; set; }

    [JsonPropertyName("outputFileName")]
    public string OutputFileName { get; set; }

    [JsonPropertyName("fileSize")]
    public long FileSize { get; set; }
    [JsonPropertyName("width")]
    public required int Width { get; set; }
    [JsonPropertyName("height")]  
    public required int Height { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("resolution")]
    public string Resolution { get; set; }

    [JsonPropertyName("mimeType")]
    public string MIMEType { get; set; }
}
