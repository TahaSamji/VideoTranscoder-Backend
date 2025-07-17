using System.Security.Claims;
using VideoTranscoder.VideoTranscoder.Application.DTOs;

namespace VideoTranscoder.VideoTranscoder.Application.Interfaces
{
    /// <summary>
    /// Interface defining authentication-related operations like sign-up, login, and extracting user ID from claims.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user and returns an authentication token upon success.
        /// </summary>
        /// <param name="dto">The user registration data (username, email, password).</param>
        /// <returns>AuthResponseDto containing JWT token and user info.</returns>
        Task<AuthResponseDto> SignUpAsync(SignUpDto dto);

        /// <summary>
        /// Authenticates a user with email and password, returns a JWT token if successful.
        /// </summary>
        /// <param name="dto">Login credentials (email and password).</param>
        /// <returns>AuthResponseDto containing JWT token and user info.</returns>
        Task<AuthResponseDto> LoginAsync(LoginDto dto);

        /// <summary>
        /// Extracts the currently authenticated user's ID from the claims principal (usually from the JWT).
        /// </summary>
        /// <param name="user">ClaimsPrincipal from the controller context (User property).</param>
        /// <returns>User ID as an integer.</returns>
        int GetCurrentUserId(ClaimsPrincipal user);
    }
}
