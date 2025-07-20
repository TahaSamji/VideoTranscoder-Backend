// using VideoTranscoder.VideoTranscoder.Application.constants;
// using VideoTranscoder.VideoTranscoder.Domain.Entities;

// namespace VideoTranscoder.VideoTranscoder.Worker.Services
// {
//     public class DashCommandBuilder : IFfmpegCommandBuilder
//     {
//         public string Build(string filePath, EncodingProfile profile, string outputDir)
//         {
//             var drawTextFilter = $"-vf \"drawtext=text='{Constants.DefaultWatermarkText}':fontcolor=white:fontsize=24:x=10:y=10\"";

//             var dashDir = Path.Combine(outputDir, "dash");
//             Directory.CreateDirectory(dashDir);

//             return $"-y -i \"{filePath}\" {drawTextFilter} {profile.FfmpegArgs} " +
//                    $"\"{dashDir}/manifest.mpd\"";
//         }
//     }

// }

using VideoTranscoder.VideoTranscoder.Application.constants;

namespace VideoTranscoder.VideoTranscoder.Worker.Services
{
    // Concrete implementation of IFfmpegCommandBuilder for DASH streaming format
    public class DashCommandBuilder : IFfmpegCommandBuilder
    {
        // Private fields to store various parts of the FFmpeg command
        private string _inputFile  = string.Empty; // Path to input video file
        private string _drawText = $"-vf \"drawtext=text='{Constants.DefaultWatermarkText}':fontcolor=white:fontsize=24:x=10:y=10\""; // Default watermark
        private string _encodingArgs  = string.Empty; // Additional FFmpeg encoding arguments
        private string _outputDir  = string.Empty; // Directory where output files will be stored

        // Sets the input video file path
        public IFfmpegCommandBuilder SetInputFile(string filePath)
        {
            _inputFile = filePath;
            return this;
        }

        // Sets additional FFmpeg encoding arguments
        public IFfmpegCommandBuilder SetEncodingArgs(string encodingArgs)
        {
            _encodingArgs = encodingArgs;
            return this;
        }

        // Overrides the default drawtext watermark with custom text
        public IFfmpegCommandBuilder SetDrawText(string text)
        {
            _drawText = $"-vf \"drawtext=text='{text}':fontcolor=white:fontsize=24:x=10:y=10\"";
            return this;
        }

        // Sets the output directory and ensures the "dash" subfolder exists
        public IFfmpegCommandBuilder SetOutputDirectory(string outputDir)
        {
            _outputDir = Path.Combine(outputDir, "dash"); // Ensures a subfolder specific to DASH
            Directory.CreateDirectory(_outputDir); // Creates the directory if it doesn't exist
            return this;
        }

        // Builds and returns the complete FFmpeg command as a string
        public string Build()
        {
            // Ensure that required fields are set before building
            if (string.IsNullOrEmpty(_inputFile))
                throw new InvalidOperationException("Input file not set.");

            if (string.IsNullOrEmpty(_outputDir))
                throw new InvalidOperationException("Output directory not set.");

            // Construct the final FFmpeg command using all configured components
            return $"-y -i \"{_inputFile}\" {_drawText} {_encodingArgs} \"{_outputDir}/manifest.mpd\"";
        }
    }
}
