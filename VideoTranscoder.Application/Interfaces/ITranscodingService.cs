

using VideoTranscoder.VideoTranscoder.Application.DTOs;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{


    public interface ITranscodingService
    {
        Task<String> TranscodeVideoAsync(TranscodeRequestMessage request, CancellationToken cancellationToken = default);
    }
}
