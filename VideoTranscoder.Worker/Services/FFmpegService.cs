using System.Diagnostics;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    public class FFmpegService
    {
        private readonly ILogger<FFmpegService> _logger;
        private readonly ICloudStorageService _cloudStorageService;

        public FFmpegService(ILogger<FFmpegService> logger, ICloudStorageService cloudStorageService)
        {
            _logger = logger;
            _cloudStorageService = cloudStorageService;
        }

        public Task<string> GenerateThumbnailAsync(string inputBlobPath, string time)
        {
            throw new NotImplementedException();
        }

        public Task<string> TranscodeAsync(string inputBlobPath, string outputPrefix, string ffmpegArgs, string formatType)
        {
            throw new NotImplementedException();
        }
    }



}