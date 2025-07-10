
namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    public interface IUserService
    {
        int UserId { get; }
        bool IsAuthenticated { get; }
    }
}