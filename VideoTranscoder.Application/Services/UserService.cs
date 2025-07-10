using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Application.Services
{
    public class UserService : IUserService
    {
        public int UserId { get; }
        public bool IsAuthenticated { get; }

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated ?? false)
            {
                var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? user.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var id))
                {
                    UserId = id;
                    IsAuthenticated = true;
                }
            }
        }
    }
}