
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using VideoTranscoder.VideoTranscoder.Application.Configurations;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Storage
{
    public class AzureBlobStorageService : ICloudStorageService
    {
        private readonly AzureOptions _azureOptions;
        private readonly BlobServiceClient _blobServiceClient;

        public AzureBlobStorageService(IOptions<AzureOptions> azureOptions, BlobServiceClient blobServiceClient)
        {
            _azureOptions = azureOptions.Value;
            _blobServiceClient = blobServiceClient;

        }

        public async Task<string> GenerateUploadSasUriAsync(string filename)
        {
            try
            {
                string containerName = _azureOptions.ContainerName;

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(filename);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = filename,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Create | BlobSasPermissions.Write);
                var sasUri = blobClient.GenerateSasUri(sasBuilder);


                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error generating SAS URI: " + ex.Message);
                throw;
            }
        }


        // New method to get blob as stream
        public async Task<Stream> GetBlobStreamAsync(string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("uploads");
                var blobClient = containerClient.GetBlobClient(fileName);

                if (!await blobClient.ExistsAsync())
                {
                    throw new FileNotFoundException($"Blob not found: {fileName}");
                }

                var response = await blobClient.OpenReadAsync();
                Console.WriteLine($"✅ Opened blob stream for: {fileName}");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in GetBlobStreamAsync: " + ex.Message);
                throw;
            }
        }

        // New method to upload thumbnail to blob storage
        public async Task<string> UploadThumbnailAsync(Stream thumbnailStream, string thumbnailFileName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("uploads");
                await containerClient.CreateIfNotExistsAsync();

                // Create thumbnail path in thumbnails directory
                var thumbnailBlobPath = $"thumbnails/{thumbnailFileName}";
                var blobClient = containerClient.GetBlobClient(thumbnailBlobPath);

                // Upload thumbnail
                thumbnailStream.Position = 0; // Reset stream position
                await blobClient.UploadAsync(thumbnailStream, overwrite: true);

                // Set content type for thumbnail
                await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
                {
                    ContentType = "image/jpeg"
                });

                Console.WriteLine($"✅ Successfully uploaded thumbnail: {thumbnailBlobPath}");
                return thumbnailBlobPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in UploadThumbnailAsync: " + ex.Message);
                throw;
            }
        }

        // New method to generate thumbnail URL (if you need public access)
        public string GenerateThumbnailSasUri(string thumbnailBlobPath, int hoursExpiry = 24)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient("uploads");
                var blobClient = containerClient.GetBlobClient(thumbnailBlobPath);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = "uploads",
                    BlobName = thumbnailBlobPath,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(hoursExpiry)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);
                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error generating thumbnail SAS URI: " + ex.Message);
                throw;
            }
        }
        // public async Task<string> UploadFileToBlob(string localFilePath, string blobPath)
        // {
        //     try
        //     {
        //         using var fileStream = File.OpenRead(localFilePath);
        //         var containerClient = _blobServiceClient.GetBlobContainerClient("uploads");
        //         var blobClient = containerClient.GetBlobClient(blobPath);

        //         await blobClient.UploadAsync(fileStream, overwrite: true);

        //         // Set appropriate content type
        //         var contentType = Path.GetExtension(blobPath).ToLower() switch
        //         {
        //             ".m3u8" => "application/vnd.apple.mpegurl",
        //             ".ts" => "video/mp2t",
        //             _ => "application/octet-stream"
        //         };

        //         await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });

        //         Console.WriteLine($"✅ Uploaded: {blobPath}");
        //         return blobPath;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"❌ Error uploading {blobPath}: {ex.Message}");
        //         throw;
        //     }
        // }

        public async Task<string> UploadFileToBlob(string localFilePath, string blobPath)
{
    try
    {
        using var fileStream = File.OpenRead(localFilePath);
        var containerClient = _blobServiceClient.GetBlobContainerClient("uploads");
        var blobClient = containerClient.GetBlobClient(blobPath);

        await blobClient.UploadAsync(fileStream, overwrite: true);

        // Auto-detect MIME type
        var extension = Path.GetExtension(blobPath).ToLowerInvariant();
        var contentType = extension switch
        {
            ".m3u8" => "application/vnd.apple.mpegurl",
            ".ts" => "video/mp2t",
            ".mp4" => "video/mp4",
            ".mpd" => "application/dash+xml",
            ".m4s" => "video/iso.segment",
            ".webm" => "video/webm",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".txt" => "text/plain",
            ".json" => "application/json",
            _ => "application/octet-stream"
        };

        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
        {
            ContentType = contentType
        });

        Console.WriteLine($"✅ Uploaded: {blobPath} as {contentType}");
        return blobPath;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error uploading {blobPath}: {ex.Message}");
        throw;
    }
}


      
    }


}