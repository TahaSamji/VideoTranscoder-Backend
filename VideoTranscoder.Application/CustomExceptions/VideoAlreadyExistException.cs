public class VideoAlreadyExistsException : InvalidOperationException
{
    public VideoAlreadyExistsException(string message) : base(message) { }
}