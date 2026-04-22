using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using WorkTracker.Common.Exceptions;

namespace WorkTracker.Common.Repositories
{
    public abstract class BaseRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        protected (int Id, string Username) GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new ApiException(HttpStatusCode.Unauthorized);
            }

            var userIdValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var username = user.Identity.Name ?? user.FindFirst(ClaimTypes.Name)?.Value;

            if (!int.TryParse(userIdValue, out var userId)
                || string.IsNullOrWhiteSpace(username))
            {
                throw new ApiException(HttpStatusCode.Unauthorized, "Invalid user.");
            }

            return (userId, username);
        }
    }
}
