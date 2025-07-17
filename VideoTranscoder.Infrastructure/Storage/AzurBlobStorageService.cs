
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
        private readonly IUserService _userService;
        private readonly ILogger<AzureBlobStorageService> _logger;



        public AzureBlobStorageService(ILogger<AzureBlobStorageService> logger, IOptions<AzureOptions> azureOptions, BlobServiceClient blobServiceClient, IUserService userService)
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
                // Get current authenticated user's ID
                int userId = _userService.UserId;

                // Build blob name based on userId and file name
                string blobName = $"{userId}/{fileName}";
                string containerName = _azureOptions.ContainerName;

                // Get reference to blob container
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Create the container if it doesn't already exist
                await containerClient.CreateIfNotExistsAsync();

                // Get reference to the specific blob
                var blobClient = containerClient.GetBlobClient(blobName);

                // Create a SAS token builder
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,   // Name of the container
                    BlobName = blobName,                 // Name of the blob
                    Resource = "b",                      // "b" stands for blob resource
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Valid from 5 minutes ago to account for clock skew
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)    // SAS token valid for 1 hour
                };

                // Set the permissions on the blob: read, create, write
                sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Create | BlobSasPermissions.Write);

                // Generate the SAS URI using the builder
                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                // Return the URI as a string
                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                // Log and rethrow any error encountered during SAS generation
                Console.WriteLine("❌ Error generating SAS URI: " + ex.Message);
                throw;
            }
        }


        public async Task<string> GenerateBlobSasUriAsync(string storagePath)
        {
            try
            {
                // Extract blob path and container name
                string blobPath = storagePath;
                string containerName = _azureOptions.ContainerName;

                // Get the container client
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure the container exists
                await containerClient.CreateIfNotExistsAsync();

                // Get the blob client for the specific file
                var blobClient = containerClient.GetBlobClient(blobPath);

                // Build SAS token with read, write, and create permissions
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobPath,
                    Resource = "b", // 'b' indicates blob-level SAS
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Slightly in the past to account for clock skew
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)    // Valid for 1 hour
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Create | BlobSasPermissions.Write);

                // Generate SAS URI
                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                _logger.LogInformation("✅ Generated SAS URI for blob: {BlobPath}", blobPath);

                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating blob SAS URI for path: {BlobPath}", storagePath);
                throw;
            }
        }

        public async Task<string> GenerateContainerSasUriAsync()
        {
            try
            {
                // Get container name from config
                string containerName = _azureOptions.ContainerName;

                // Get container client
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure container exists
                await containerClient.CreateIfNotExistsAsync();

                // Create a SAS builder for container-level access
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    Resource = "c", // "c" indicates container-level SAS
                    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Buffer to account for clock skew
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)    // 1-hour validity
                };

                // Grant read permissions for the entire container (sufficient for video playback)
                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read);

                // Generate the SAS URI
                var sasUri = containerClient.GenerateSasUri(sasBuilder);

                _logger.LogInformation("✅ Generated container-level SAS URI: {SasUri}", sasUri);
                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                // Error Creating SAS URI
                _logger.LogError(ex, "❌ Error generating container SAS URI");
                throw;
            }
        }

        public async Task<string> UploadTranscodedOutputAsync(string tempOutputDir, string fileName, int fileId, int userId, int encodingProfileId)
        {
            try
            {
                // Check if the local temp output directory exists
                if (!Directory.Exists(tempOutputDir))
                    throw new NotFoundException($"❌ Temp output directory not found: {tempOutputDir}");

                string containerName = _azureOptions.ContainerName;
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure the container exists
                await containerClient.CreateIfNotExistsAsync();

                string[] formats = ["hls", "dash"];
                string? firstUploadedBlobPath = null;

                foreach (var format in formats)
                {
                    var subDir = Path.Combine(tempOutputDir, format);

                    // Skip if format directory doesn't exist
                    if (!Directory.Exists(subDir))
                    {
                        _logger.LogWarning("⚠️ Skipped: Directory not found for format '{Format}': {SubDir}", format, subDir);
                        continue;
                    }

                    var files = Directory.GetFiles(subDir, "*", SearchOption.AllDirectories);
                    string transcodedPath = $"{userId}/{fileName}_{fileId}/{encodingProfileId}";

                    foreach (var localFilePath in files)
                    {
                        string? blobPath = null;

                        try
                        {
                            // Create a relative blob path
                            var relativePath = Path.GetRelativePath(tempOutputDir, localFilePath).Replace("\\", "/");
                            blobPath = $"{transcodedPath}/{relativePath}";

                            // Upload the file to blob
                            using var fileStream = File.OpenRead(localFilePath);
                            var blobClient = containerClient.GetBlobClient(blobPath);

                            await blobClient.UploadAsync(fileStream, overwrite: true);

                            // Set the appropriate content type
                            var contentType = Path.GetExtension(blobPath).ToLower() switch
                            {
                                ".m3u8" => "application/vnd.apple.mpegurl",
                                ".mpd" => "application/dash+xml",
                                ".m4s" => "video/iso.segment",
                                ".mp4" => "video/mp4",
                                _ => "application/octet-stream"
                            };

                            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType });

                            _logger.LogInformation("✅ Uploaded: {BlobPath}", blobPath);

                            // Save the path of the first successfully uploaded file
                            firstUploadedBlobPath ??= transcodedPath;
                        }
                        catch (Exception ex)
                        {
                            // Log and continue on file-level failure
                            _logger.LogError(ex, "❌ Failed to upload file '{LocalPath}' to '{BlobPath}'", localFilePath, blobPath);
                        }
                    }
                }

                // Uncomment if you want to clean local files afterward
                // await _cleanerService.CleanDirectoryContentsAsync(tempOutputDir);

                // Ensure at least one file was uploaded
                if (firstUploadedBlobPath == null)
                    throw new Exception("❌ Upload failed: No files were uploaded to blob storage.");

                return firstUploadedBlobPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Fatal error in UploadTranscodedOutputAsync");
                throw;
            }
        }


        public async Task<string> DownloadVideoToLocalAsync(string filename, int userId, int fileId)
        {
            string currentDir = Directory.GetCurrentDirectory();
            string inputDir = Path.Combine(currentDir, "input", $"{userId}", $"{fileId}");
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
                // Get container name and user ID
                string containerName = _azureOptions.ContainerName;
                int userId = _userService.UserId;

                // Build the blob path for the thumbnail
                var thumbnailBlobPath = $"{userId}/{fileName}_{fileId}/thumbnails/{thumbnailFileName}";

                // Get blob container client
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Create the container if it doesn't exist
                await containerClient.CreateIfNotExistsAsync();

                // Get the blob client for the specific thumbnail
                var blobClient = containerClient.GetBlobClient(thumbnailBlobPath);

                // Reset stream position and upload the thumbnail to the blob
                thumbnailStream.Position = 0;
                await blobClient.UploadAsync(thumbnailStream, overwrite: true);

                // Set appropriate content type
                await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
                {
                    ContentType = "image/jpeg"
                });

                // Log successful upload
                _logger.LogInformation("✅ Successfully uploaded thumbnail: {BlobPath}", thumbnailBlobPath);

                return thumbnailBlobPath;
            }
            catch (Exception ex)
            {
                // Log error and rethrow
                _logger.LogError(ex, "❌ Error in UploadThumbnailAsync for fileId: {FileId}, fileName: {FileName}", fileId, fileName);
                throw;
            }
        }
        public async Task<List<string>> UploadThumbnailsFromDirectoryAsync(string localDirectoryPath, int fileId, string originalFileName, int userId)
        {
            var uploaded = new List<string>();
            try
            {
                // Check if the thumbnail directory exists
                if (!Directory.Exists(localDirectoryPath))
                    throw new NotFoundException($"Thumbnail directory not found: {localDirectoryPath}");

                // Get container name from configuration
                string containerName = _azureOptions.ContainerName;

                // Get reference to the blob container
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Ensure the container exists or create it if it doesn't
                await containerClient.CreateIfNotExistsAsync();

                // Construct the blob path where thumbnails will be uploaded
                string thumbnailDirectoryBlobPath = $"{userId}/{originalFileName}_{fileId}/thumbnails";

                // Get all thumbnail file paths from the local directory
                var thumbnailFiles = Directory.GetFiles(localDirectoryPath);

                // Iterate over each thumbnail file and upload it
                foreach (var thumbnailPath in thumbnailFiles)
                {
                    var fileName = Path.GetFileName(thumbnailPath); // Extract filename from path
                    var blobPath = $"{thumbnailDirectoryBlobPath}/{fileName}"; // Complete blob path for each thumbnail
                    var blobClient = containerClient.GetBlobClient(blobPath); // Get blob client

                    await using var stream = File.OpenRead(thumbnailPath); // Open the file stream
                    stream.Position = 0; // Reset stream position in case it's not at the beginning

                    // Upload the stream to Azure Blob Storage with overwrite enabled
                    await blobClient.UploadAsync(stream, overwrite: true);

                    // Set the content type for the blob as JPEG
                    await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
                    {
                        ContentType = "image/jpeg"
                    });

                    // Add the uploaded blob path to the result list
                    uploaded.Add(blobPath);

                    // Log success (can be replaced with ILogger if needed)
                    _logger.LogInformation($"✅ Uploaded thumbnail: {blobPath}");
                }

                return uploaded;
            }
            catch (Exception ex)
            {
                // Log error and rethrow
                _logger.LogInformation("❌ Error in UploadThumbnailsFromDirectoryAsync: {message} " + ex.Message);
                throw;
            }
        }



        public string GenerateThumbnailSasUri(string thumbnailBlobPath, int hoursExpiry = 24)
        {
            try
            {
                // Retrieve the name of the container from configuration
                string containerName = _azureOptions.ContainerName;

                // Get reference to the container
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

                // Get reference to the specific thumbnail blob
                var blobClient = containerClient.GetBlobClient(thumbnailBlobPath);

                // Create a new SAS builder to define the access parameters
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,                   // Set the target container
                    BlobName = thumbnailBlobPath,                        // Set the target blob path
                    Resource = "b",                                      // "b" denotes that the resource is a blob
                    StartsOn = DateTimeOffset.UtcNow,                    // SAS token becomes valid immediately
                    ExpiresOn = DateTimeOffset.UtcNow.AddHours(hoursExpiry) // SAS token expiration
                };

                // Grant read and write permissions for the blob
                sasBuilder.SetPermissions(BlobSasPermissions.Read | BlobSasPermissions.Write);

                // Generate the full URI with SAS token
                var sasUri = blobClient.GenerateSasUri(sasBuilder);

                // Return the URI as a string
                return sasUri.ToString();
            }
            catch (Exception ex)
            {
                // Log and rethrow the exception in case of failure
                Console.WriteLine("❌ Error generating thumbnail SAS URI: " + ex.Message);
                throw;
            }
        }







    }


}