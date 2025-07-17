using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    // DTO used to carry login credentials (email and password) from client to server.
    public class LoginDto
    {
        [Required(ErrorMessage = "Email is required.")] // Ensures the email field is not empty
        [EmailAddress(ErrorMessage = "Invalid email format.")] // Validates the format is a proper email
        public required string Email { get; set; } // User's login identifier

        [Required(ErrorMessage = "Password is required.")] // Ensures the password field is not empty
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")] // Minimum password length
        public required string Password { get; set; } // User's secret key for authentication
    }
}
