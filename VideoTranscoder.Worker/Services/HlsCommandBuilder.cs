// using VideoTranscoder.VideoTranscoder.Application.constants;
// using VideoTranscoder.VideoTranscoder.Domain.Entities;

// namespace VideoTranscoder.VideoTranscoder.Worker.Services
// {
//     public class HlsCommandBuilder : IFfmpegCommandBuilder
//     {
//         public string Build(string filePath, EncodingProfile profile, string outputDir)
//         {
//             var hlsDir = Path.Combine(outputDir, "hls");
//             Directory.CreateDirectory(hlsDir);
//             var drawTextFilter = $"-vf \"drawtext=text='{Constants.DefaultWatermarkText}':fontcolor=white:fontsize=24:x=10:y=10\"";


//             return $"-y -i \"{filePath}\" {drawTextFilter} {profile.FfmpegArgs} " +
//                    $"-hls_segment_filename \"{hlsDir}/segment_%03d.m4s\" " +
//                    $"\"{hlsDir}/playlist.m3u8\"";
//         }
//     }

// }

using VideoTranscoder.VideoTranscoder.Application.constants;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    // Concrete implementation of the FFmpeg command builder for HLS format
    public class HlsCommandBuilder : IFfmpegCommandBuilder
    {
        // Path to the input video file
        private string _inputFile = string.Empty;

        // FFmpeg drawtext filter with default watermark text
        private string _drawText = string.Empty;



        // Additional FFmpeg encoding arguments (bitrate, resolution, etc.)
        private string _encodingArgs = string.Empty;

        // Directory where the HLS output will be saved
        private string _outputDir = string.Empty;

        // Sets the input video file path
        public IFfmpegCommandBuilder SetInputFile(string filePath)
        {
            _inputFile = filePath;
            return this;
        }

        // Sets custom watermark text for the drawtext filter
        public IFfmpegCommandBuilder SetDrawText(string text)
        {
        _drawText = $"-vf \"drawtext=text='{Constants.DefaultWatermarkText}':fontfile='C\\:/Windows/Fonts/arialbd.ttf':fontcolor=white:fontsize=24:x=10:y=10\"";




            return this;
        }

        // Sets encoding arguments (e.g., -b:v 1000k -s 1280x720)
        public IFfmpegCommandBuilder SetEncodingArgs(string encodingArgs)
        {
            _encodingArgs = encodingArgs;
            return this;
        }

        // Sets the output directory and creates an "hls" subfolder within it
        public IFfmpegCommandBuilder SetOutputDirectory(string outputPath)
        {
            _outputDir = Path.Combine(outputPath, "hls");
            Directory.CreateDirectory(_outputDir); // Ensure the output folder exists
            return this;
        }

        // Builds the final FFmpeg command string for HLS output
        public string Build()
        {
            // return $"-y -i \"{_inputFile}\" {_drawText} {_encodingArgs} " +
            //        $"-hls_segment_filename \"{_outputDir}/segment_%03d.m4s\" " +
            //        $"\"{_outputDir}/playlist.m3u8\"";
            return $"-y -i \"{_inputFile}\" {_drawText} {_encodingArgs} " +
                             $"-hls_segment_filename \"{_outputDir}/segment_%03d.m4s\" " +
                             $"\"{_outputDir}/playlist.m3u8\"";
        }
    }
}
