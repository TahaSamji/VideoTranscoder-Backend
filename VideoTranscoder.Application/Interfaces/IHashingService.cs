namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IHashingService
    {
        // Takes a plain text password and returns a hashed version (e.g., using SHA256, BCrypt, etc.)
        string HashPassword(string password);
    }
}
