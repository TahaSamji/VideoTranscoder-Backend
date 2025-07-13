
namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface ICDNService
    {
        /// <summary>
        /// Generates a signed CDN URL for a blob path.
        /// </summary>
        /// <param name="storagePath">The blob storage path (e.g., container/video.mp4).</param>
        /// <returns>Signed URL that maps to the CDN front door endpoint.</returns>
        Task<string> GenerateSignedUrlAsync(string storagePath);
    }
}