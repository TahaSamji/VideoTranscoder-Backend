using System.ComponentModel.DataAnnotations;

namespace VideoTranscoder.VideoTranscoder.Application.DTOs
{
    // DTO used for user registration. Captures username, email, and password during signup.
    public class SignUpDto
    {
        [Required(ErrorMessage = "Username is required.")] // Validates presence of username
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters long.")] // Ensures username is not too short
        [MaxLength(50, ErrorMessage = "Username cannot exceed 50 characters.")] // Prevents overly long usernames
        public required string Username { get; set; } // User's display or handle name

        [Required(ErrorMessage = "Email is required.")] // Validates that email is provided
        [EmailAddress(ErrorMessage = "Invalid email format.")] // Ensures proper email format
        public required string Email { get; set; } // Used as the unique identifier for login

        [Required(ErrorMessage = "Password is required.")] // Validates presence of password
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")] // Enforces a minimum password strength
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")] // Prevents extremely long passwords
        public required string Password { get; set; } // The user's secure login password
    }
}
