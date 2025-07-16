
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using VideoTranscoder.VideoTranscoder.Application.Configurations;
using VideoTranscoder.VideoTranscoder.Application.enums;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Application.Services;
using VideoTranscoder.VideoTranscoder.Worker.Services;

namespace VideoTranscoder.VideoTranscoder.Infrastructure.Storage
{
    public class AzureBlobStorageService : ICloudStorageService
    {
        private readonly AzureOptions _azureOptions;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IUserService _userService;
        private readonly ILogger<AzureBlobStorageService> _logger;



        public AzureBlobStorageService( ILogger<AzureBlobStorageService> logger, IOptions<AzureOptions> azureOptions, BlobServiceClient blobServiceClient, IUserService userService)
        {
            _azureOptions = azureOptions.Value;
            _blobServiceClient = blobServiceClient;
            _userService = userService;
            _logger = logger;
        }

        public async Task<string> GenerateSasUriAsync(string fileName)
        {
            try
            {
                int userId = _userService.UserId;
                string blobName = $"{userId}/{fileName}";
                string containerName = _azureOptions.ContainerName;

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(blobName);


                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
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

        public async Task<string> GenerateBlobSasUriAsync(string StoragePath)
        {
            try
            {

                string blobPath = StoragePath;
                string containerName = _azureOptions.ContainerName;

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                var blobClient = containerClient.GetBlobClient(blobPath);


                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobPath,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
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

        public async Task<string> GenerateContainerSasUriAsync()
        {
            try
            {
                string containerName = _azureOptions.ContainerName;

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    Resource = "c", // 'c' means container-level SAS
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
                };

                // Only READ access is needed for playback
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                var sasUri = containerClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error generating container SAS URI: " + ex.Message);
                throw;
            }
        }


            public async Task<string> UploadTranscodedOutputAsync(string tempOutputDir, string fileName, int fileId, int userId, int encodingProfileId)
        {
            try
            {
                if (!Directory.Exists(tempOutputDir))
                    throw new DirectoryNotFoundException($"❌ Temp output directory not found: {tempOutputDir}");

                string containerName = _azureOptions.ContainerName;
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                await containerClient.CreateIfNotExistsAsync();

                string[] formats = ["hls", "dash"];
                string? firstUploadedBlobPath = null;

                foreach (var format in formats)
                {
                    var subDir = Path.Combine(tempOutputDir, format);

                    if (!Directory.Exists(subDir))
                    {
                        Console.WriteLine($"⚠️ Skipped: Directory not found for format '{format}': {subDir}");
                        continue;
                    }

                    var files = Directory.GetFiles(subDir, "*", SearchOption.AllDirectories);
                    string transcodedPath = $"{userId}/{fileName}_{fileId}/{encodingProfileId}";

                    foreach (var localFilePath in files)
                    {
                        string? blobPath = null;

                        try
                        {
                            var relativePath = Path.GetRelativePath(tempOutputDir, localFilePath).Replace("\\", "/");
                            blobPath = $"{transcodedPath}/{relativePath}";

                            using var fileStream = File.OpenRead(localFilePath);
                            var blobClient = containerClient.GetBlobClient(blobPath);

                            await blobClient.UploadAsync(fileStream, overwrite: true);

                            var contentType = Path.GetExtension(blobPath).ToLower() switch
                            {
                                ".m3u8" => "application/vnd.apple.mpegurl",
                                ".mpd" => "application/dash+xml",
                                ".m4s" => "video/iso.segment",
                                ".mp4" => "video/mp4",
                                _ => "application/octet-stream"
                            };

                            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });

                            Console.WriteLine($"✅ Uploaded: {blobPath}");

                            // Save first uploaded blob path
                            firstUploadedBlobPath ??= transcodedPath;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"❌ Failed to upload file '{localFilePath}' to '{blobPath}': {ex.Message}");
                            // Don't throw here — continue with next file
                        }
                    }
                }
                //  await _cleanerService.CleanDirectoryContentsAsync(tempOutputDir);

                if (firstUploadedBlobPath == null)
                    throw new Exception("❌ Upload failed: No files were uploaded to blob storage.");

                return firstUploadedBlobPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Fatal error in UploadTranscodedOutputAsync: {ex.Message}");
                throw;
            }
        }


        public async Task<string> DownloadVideoToLocalAsync(string filename, int userId, int fileId)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string inputDir = Path.Combine(currentDir, "input", $"{userId}", $"{fileId}", "videos");
            string localFilePath = Path.Combine(inputDir, filename);

            try
            {
                _logger.LogInformation("⬇️ Starting download for file: {Filename}, user: {UserId}, fileId: {FileId}", filename, userId, fileId);

                // Create directory if it doesn't exist
                Directory.CreateDirectory(inputDir);

                // Generate SAS URL for the blob
                var sasUrl = await GenerateSasUriAsync(filename);

                // Use HttpClient to download the file
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(sasUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                // Write the content to a local file
                await using var fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);

                _logger.LogInformation("✅ Download completed: {LocalPath}", localFilePath);
                return localFilePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error downloading video: {Filename} for user {UserId}, file {FileId}", filename, userId, fileId);
                throw;
            }
        }

        // New method to upload thumbnail to blob storage
        public async Task<string> UploadThumbnailAsync(Stream thumbnailStream, string thumbnailFileName, int fileId, string fileName)
        {
            try
            {
                string containerName = _azureOptions.ContainerName;
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();
                int userId = _userService.UserId;
                // Create thumbnail path in thumbnails directory
                var thumbnailBlobPath = $"{userId}/{fileName}_{fileId}/thumbnails/{thumbnailFileName}";
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
        public async Task<List<string>> UploadThumbnailsFromDirectoryAsync(string localDirectoryPath, int fileId, string originalFileName, int userId)
        {
            var uploaded = new List<string>();
            try
            {
                if (!Directory.Exists(localDirectoryPath))
                    throw new DirectoryNotFoundException($"Thumbnail directory not found: {localDirectoryPath}");

                string containerName = _azureOptions.ContainerName;
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                string thumbnailDirectoryBlobPath = $"{userId}/{originalFileName}_{fileId}/thumbnails";

                var thumbnailFiles = Directory.GetFiles(localDirectoryPath);

                foreach (var thumbnailPath in thumbnailFiles)
                {
                    var fileName = Path.GetFileName(thumbnailPath);
                    var blobPath = $"{thumbnailDirectoryBlobPath}/{fileName}";
                    var blobClient = containerClient.GetBlobClient(blobPath);

                    await using var stream = File.OpenRead(thumbnailPath);
                    stream.Position = 0;

                    await blobClient.UploadAsync(stream, overwrite: true);

                    await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
                    {
                        ContentType = "image/jpeg"
                    });

                    uploaded.Add(blobPath);
                    Console.WriteLine($"✅ Uploaded thumbnail: {blobPath}");
                }

                return uploaded;
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error in UploadThumbnailsFromDirectoryAsync: " + ex.Message);
                throw;
            }
        }



        public string GenerateThumbnailSasUri(string thumbnailBlobPath, int hoursExpiry = 24)
        {
            try
            {
                string containerName = _azureOptions.ContainerName;
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(thumbnailBlobPath);

                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = thumbnailBlobPath,
                    Resource = "b",
                    StartsOn = DateTimeOffset.UtcNow,
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(hoursExpiry)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);
                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error generating thumbnail SAS URI: " + ex.Message);
                throw;
            }
        }






    }


}