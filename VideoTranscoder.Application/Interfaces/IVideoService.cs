

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{


    public interface IVideoService
    {
        Task<string> StoreFileAndReturnThumbnailUrlAsync( int totalChunks, string outputFileName, int userId,long fileSize,int EncodingId);
    }
}