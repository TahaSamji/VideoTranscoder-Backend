// Interface update - IAzureService.cs
namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface ICloudStorageService
    {
        Task<string> GenerateSasUriAsync(string filename);
        Task<string> UploadThumbnailAsync(Stream thumbnailStream, string thumbnailFileName);
        string GenerateThumbnailSasUri(string thumbnailBlobPath, int hoursExpiry = 24);
        Task<string> DownloadVideoToLocalAsync(string filename,int userId,int fileId);
        Task<string> UploadTranscodedOutputAsync(string tempOutputDir, string fileName,int fileId,int userId,int encodingProfileId);
    }
}