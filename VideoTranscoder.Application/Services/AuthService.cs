using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using VideoTranscoder.VideoTranscoder.Application.DTOs;
using VideoTranscoder.VideoTranscoder.Application.enums;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;
using VideoTranscoder.VideoTranscoder.Domain.DatabaseContext;
using VideoTranscoder.VideoTranscoder.Domain.Entities;

namespace VideoTranscoder.VideoTranscoder.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtService _jwtService;
        private readonly IHashingService _hashingService;


        // Constructor injecting DbContext and JWT service
        public AuthService(AppDbContext context, JwtService jwtService, IHashingService hashingService)
        {
            _context = context;
            _jwtService = jwtService;
            _hashingService = hashingService;
        }

        // Handles user registration
        public async Task<AuthResponseDto> SignUpAsync(SignUpDto dto)
        {
            // Check if user with given email already exists
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new Exception("User already exists");

            // Create a new user with hashed password and default role
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = _hashingService.HashPassword(dto.Password),
                Role = UserRole.User 
            };

            // Save new user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Return response with JWT token
            return new AuthResponseDto
            {
                Token = _jwtService.GenerateToken(user),
                Email = user.Email,
                Username = user.Username
            };
        }

        // Handles user login
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            // Check if user exists and password is correct
            if (user == null || user.PasswordHash != _hashingService.HashPassword(dto.Password))
                throw new Exception("Invalid credentials");

            // Return response with JWT token
            return new AuthResponseDto
            {
                Token = _jwtService.GenerateToken(user),
                Email = user.Email,
                Username = user.Username
            };
        }

      
        // Extracts and returns the current user's ID from JWT claims
        public int GetCurrentUserId(ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)
                            ?? user.FindFirst(ClaimTypes.NameIdentifier); // fallback

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                throw new UnauthorizedAccessException("Invalid or missing user ID claim.");

            return userId;
        }
    }
}
