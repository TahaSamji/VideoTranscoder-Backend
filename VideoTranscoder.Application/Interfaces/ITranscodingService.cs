using VideoTranscoder.VideoTranscoder.Application.DTOs;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface ITranscodingService
    {
        // Starts the video transcoding process based on the provided request message.
        // Returns the path to the transcoded output or a status message.
        Task<string> TranscodeVideoAsync(TranscodeRequestMessage request, CancellationToken cancellationToken = default);
    }
}
