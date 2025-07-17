using System.Security.Cryptography;
using System.Text;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Application.Services
{
    public class SHAHashingService : IHashingService
    {     

        // Encrypt Password Using SHA
        public string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
