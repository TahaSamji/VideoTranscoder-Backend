namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IUserService
    {
        // Gets the current user's ID from the context or authentication token.
        int UserId { get; }

        // Indicates whether the current user is authenticated.
        bool IsAuthenticated { get; }
    }
}
