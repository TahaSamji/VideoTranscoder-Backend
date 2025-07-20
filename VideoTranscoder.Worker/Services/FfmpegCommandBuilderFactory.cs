
// namespace VideoTranscoder.VideoTranscoder.Worker.Services
// {
//     public static class FfmpegCommandBuilderFactory
//     {
//         public static IFfmpegCommandBuilder GetBuilder(string formatType)
//         {
//             return formatType.ToLower() switch
//             {
//                 "dash" => new DashCommandBuilder(),
//                 "hls" => new HlsCommandBuilder(),
//                 _ => throw new NotSupportedException($"Unsupported format type: {formatType}")
//             };
//         }
//     }

// }

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    // Factory class responsible for creating FFmpeg command builders based on the specified format type
    public class FfmpegCommandBuilderFactory
    {
        // Static method to create an appropriate IFfmpegCommandBuilder based on formatType ("hls" or "dash")
        public static IFfmpegCommandBuilder Create(string formatType)
        {
            // Use a switch expression to return the appropriate command builder implementation
            return formatType.ToLower() switch
            {
                // If formatType is "hls", return an instance of HlsCommandBuilder
                "hls" => new HlsCommandBuilder(),

                // If formatType is "dash", return an instance of DashCommandBuilder
                "dash" => new DashCommandBuilder(),

                // If the format is not recognized, throw an exception
                _ => throw new ArgumentException($"Unsupported format: {formatType}")
            };
        }
    }
}
