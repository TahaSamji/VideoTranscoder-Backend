namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    /// <summary>
    /// Interface for cloud storage operations like SAS generation, uploading/downloading videos and thumbnails.
    /// </summary>
    public interface ICloudStorageService
    {
        /// <summary>
        /// Generates a SAS URI to allow clients to upload a file directly to blob storage.
        /// </summary>
        /// <param name="filename">Name of the blob/file to be uploaded.</param>
        /// <returns>SAS URI as a string.</returns>
        Task<string> GenerateSasUriAsync(string filename);

        /// <summary>
        /// Uploads a thumbnail image to a cloud blob container.
        /// </summary>
        /// <param name="thumbnailStream">Stream of the image file.</param>
        /// <param name="thumbnailFileName">Name of the thumbnail file.</param>
        /// <param name="videoId">ID of the video file the thumbnail is related to.</param>
        /// <param name="fileName">Original file name of the video.</param>
        /// <returns>Public or SAS URL of the uploaded thumbnail.</returns>
        Task<string> UploadThumbnailAsync(Stream thumbnailStream, string thumbnailFileName, int videoId, string fileName);

        /// <summary>
        /// Generates a SAS URI for accessing a thumbnail for a limited time.
        /// </summary>
        /// <param name="thumbnailBlobPath">Path to the thumbnail blob.</param>
        /// <param name="hoursExpiry">SAS token expiration in hours (default: 24).</param>
        /// <returns>Temporary SAS URI for the thumbnail.</returns>
        string GenerateThumbnailSasUri(string thumbnailBlobPath, int hoursExpiry = 24);

        /// <summary>
        /// Downloads the original uploaded video from blob storage to a local path for processing.
        /// </summary>
        /// <param name="filename">File name of the uploaded video.</param>
        /// <param name="userId">User ID who uploaded the video.</param>
        /// <param name="fileId">File ID from the database.</param>
        /// <returns>Local file path of the downloaded video.</returns>
        Task<string> DownloadVideoToLocalAsync(string filename, int userId, int fileId);

        /// <summary>
        /// Uploads the final transcoded video output (HLS/DASH/CMAF) from a local directory to blob storage.
        /// </summary>
        /// <param name="tempOutputDir">Path to the local folder containing transcoded files.</param>
        /// <param name="fileName">Original file name.</param>
        /// <param name="fileId">File ID from the database.</param>
        /// <param name="userId">User ID.</param>
        /// <param name="encodingProfileId">Encoding profile used to generate this output.</param>
        /// <returns>Base blob path to the uploaded rendition.</returns>
        Task<string> UploadTranscodedOutputAsync(string tempOutputDir, string fileName, int fileId, int userId, int encodingProfileId);

        /// <summary>
        /// Uploads all thumbnails from a given local directory and returns the list of blob URLs.
        /// </summary>
        /// <param name="localDirectoryPath">Local path containing thumbnails.</param>
        /// <param name="fileId">Associated video file ID.</param>
        /// <param name="originalFileName">Original video file name.</param>
        /// <param name="userId">Uploader's user ID.</param>
        /// <returns>List of blob URLs of uploaded thumbnails.</returns>
        Task<List<string>> UploadThumbnailsFromDirectoryAsync(string localDirectoryPath, int fileId, string originalFileName, int userId);

        /// <summary>
        /// Generates a SAS URI for a specific blob path.
        /// </summary>
        /// <param name="StoragePath">Relative path of the blob (e.g., `videos/123/file.mp4`).</param>
        /// <returns>SAS URL string.</returns>
        Task<string> GenerateBlobSasUriAsync(string StoragePath);

        /// <summary>
        /// Generates a SAS URI for the entire container (used rarely, for broad access).
        /// </summary>
        /// <returns>Container-level SAS URI.</returns>
        Task<string> GenerateContainerSasUriAsync();
    }
}
