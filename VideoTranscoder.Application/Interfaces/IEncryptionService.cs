
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IEncryptionService
    {
        Task<string> EncryptToHLSWithCENCAsync(string input, int userId, int fileId, EncodingProfile profile);
        Task<string> EncryptWithCENCAsync(string inputDir, int userId, int fileId, EncodingProfile encodingProfile);
    }
}