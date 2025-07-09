// Interface update - IAzureService.cs
namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface ICloudStorageService
    {
        Task<string> GenerateUploadSasUriAsync(string filename);
        Task<string> UploadThumbnailAsync(Stream thumbnailStream, string thumbnailFileName);
        Task<Stream> GetBlobStreamAsync(string blobPath);
        // Task<string> UploadThumbnailAsync(Stream thumbnailStream, string thumbnailFileName);
        string GenerateThumbnailSasUri(string thumbnailBlobPath, int hoursExpiry = 24);
        Task<string> UploadFileToBlob(string localFilePath, string blobPath);
    }
}