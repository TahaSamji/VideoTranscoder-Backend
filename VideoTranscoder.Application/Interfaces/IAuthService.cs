using System.Security.Claims;
using VideoTranscoder.VideoTranscoder.Application.DTOs;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{

    public interface IAuthService
    {
        Task<AuthResponseDto> SignUpAsync(SignUpDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        public int GetCurrentUserId(ClaimsPrincipal user);
    }
}