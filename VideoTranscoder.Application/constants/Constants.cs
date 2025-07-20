namespace VideoTranscoder.VideoTranscoder.Application.constants
{
    public static class Constants
    {
        // General Constants
        public const string DefaultWatermarkText = "© VIDIZMO";
        public const string DefaultDateFormat = "yyyy-MM-dd HH:mm:ss";
        public const string tempFolder = "temp";
        public const string hlsFolder = "hls";
        public const string dashFolder = "dash";
        public const string ffmpegPath = "ffmpeg";
        public const string dashManifest = "manifest.mpd";
        public const string hlsManifest = "playlist.m3u8";
        public const string Success = "Success";
        public const string Failure = "Failure";
        internal static object thumbnailsFolder = "thumbnails";

        // FFmpeg Settings
    }
}
