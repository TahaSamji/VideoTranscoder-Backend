using System.ComponentModel.DataAnnotations;
using VideoTranscoder.VideoTranscoder.Application.enums;

namespace VideoTranscoder.VideoTranscoder.Domain.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
        public UserRole Role { get; set; } = UserRole.User;
    }
}
