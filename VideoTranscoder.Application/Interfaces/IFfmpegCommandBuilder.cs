// using VideoTranscoder.VideoTranscoder.Domain.Entities;

// public interface IFfmpegCommandBuilder
// {
//     string Build(string filePath, EncodingProfile profile, string outputDir);
// }
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    public interface IFfmpegCommandBuilder
    {
        IFfmpegCommandBuilder SetInputFile(string filePath);
        IFfmpegCommandBuilder SetEncodingArgs(string encodingArgs);
        IFfmpegCommandBuilder SetDrawText(string text);
        IFfmpegCommandBuilder SetOutputDirectory(string outputDir);
        string Build();
    }
}
