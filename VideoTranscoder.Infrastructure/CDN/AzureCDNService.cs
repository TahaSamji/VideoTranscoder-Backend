

using Microsoft.Extensions.Options;
using VideoTranscoder.VideoTranscoder.Application.Configurations;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

public class AzureCDNService : ICDNService
{
    private readonly AzureOptions _azureOptions;
    private readonly ICloudStorageService _cloudStorageService;
    private readonly string _cdnBaseUrl;
    public AzureCDNService(IOptions<CDNOptions> cdnOptions, IOptions<AzureOptions> azureOptions, ICloudStorageService cloudStorageService)
    {
        _azureOptions = azureOptions.Value;
        _cloudStorageService = cloudStorageService;
        _cdnBaseUrl = cdnOptions.Value.BaseUrl;
    }

    // public async Task<string> GenerateSignedUrlAsync(string storagePath)
    // {
    //     // Step 1: Get the full SAS URL from blob (e.g., https://account.blob.core.windows.net/container/file.mp4?sv=...)
    //     var fullBlobSasUrl = await _cloudStorageService.GenerateBlobSasUriAsync(storagePath);

    //     var uri = new Uri(fullBlobSasUrl);

    //     // Step 2: Extract the path and query string
    //     var path = uri.AbsolutePath.TrimStart('/');  // "container/file.mp4"
    //     var sasToken = uri.Query.TrimStart('?');     // "sv=2023-08-04&..."

    //     // Step 3: Combine with CDN base URL
    //     var cdnSignedUrl = $"{_cdnBaseUrl}/{path}?{sasToken}";
    //     return cdnSignedUrl;
    // }
    public async Task<string> GenerateSignedUrlAsync(string storagePath)
    {
        // Step 1: Generate a container-level SAS token (not for a single file)
        var containerSasUrl = await _cloudStorageService.GenerateContainerSasUriAsync(); // You need to return this with token only
        var uri = new Uri(containerSasUrl);
        Console.WriteLine(uri);
        // Step 2: Extract only the query (SAS token)
        var sasToken = uri.Query.TrimStart('?');  // "sv=...&se=...&sp=...&sig=..."
        Console.WriteLine(sasToken);
        // Step 3: Combine the storagePath (e.g. uploads/1/abc/dash/manifest.mpd) with CDN base URL
        var cdnSignedUrl = $"{_cdnBaseUrl}/{storagePath}?{sasToken}";
        return cdnSignedUrl;
    }


}