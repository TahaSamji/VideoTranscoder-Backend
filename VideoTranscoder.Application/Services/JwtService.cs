using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        // Injecting configuration to access JWT settings
        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        // Generates a JWT token for the given user
        public string GenerateToken(User user)
        {
            // Define claims to include in the token (subject, email, username, role)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject: User ID
                new Claim(JwtRegisteredClaimNames.Email, user.Email),       // User email
                new Claim("username", user.Username),                       // Custom claim: username
                new Claim("role", user.Role.ToString())                     // Custom claim: user role
            };

            // Generate a symmetric security key from the secret key stored in config
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            // Create signing credentials using the key and HMAC-SHA256 algorithm
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Build the JWT token with issuer, audience, claims, expiration, and credentials
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],       // JWT issuer
                audience: _config["Jwt:Audience"],   // JWT audience
                claims: claims,                      // Claims for this user
                expires: DateTime.UtcNow.AddDays(7), // Token expiry set to 7 days
                signingCredentials: creds            // Signature credentials
            );

            // Serialize the token to a string and return it
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}