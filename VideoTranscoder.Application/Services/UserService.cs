using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using VideoTranscoder.VideoTranscoder.Application.Interfaces;

namespace VideoTranscoder.VideoTranscoder.Application.Services
{
    public class UserService : IUserService
    {
        // Holds the authenticated user's ID
        public int UserId { get; }

        // Indicates if the current user is authenticated
        public bool IsAuthenticated { get; }

        // Constructor that extracts user info from the current HTTP context
        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            // Retrieve the user principal from the current HTTP context
            var user = httpContextAccessor.HttpContext?.User;

            // Check if the user is authenticated
            if (user?.Identity?.IsAuthenticated ?? false)
            {
                // Try to get the user ID from either JWT's 'sub' claim or from the NameIdentifier claim
                var userIdClaim = user.FindFirst(JwtRegisteredClaimNames.Sub)
                                  ?? user.FindFirst(ClaimTypes.NameIdentifier);

                // If a valid claim is found and can be parsed to an integer, store the ID and mark as authenticated
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var id))
                {
                    UserId = id;
                    IsAuthenticated = true;
                }
            }
        }
    }
}
