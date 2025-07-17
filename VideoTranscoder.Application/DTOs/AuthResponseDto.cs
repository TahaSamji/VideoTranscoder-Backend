using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    // DTO returned after a successful authentication request (e.g., login)
    public class AuthResponseDto
    {
        [Required] // Required: JWT or access token for authenticated requests
        public string Token { get; set; } = string.Empty; // JWT token string

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")] // Must be a valid email address
        public string Email { get; set; } = string.Empty; // User's email address

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 100 characters")]
        public string Username { get; set; } = string.Empty; // Username displayed in UI or used for identification
    }
}
