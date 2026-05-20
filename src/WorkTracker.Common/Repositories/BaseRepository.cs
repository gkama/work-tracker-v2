using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using WorkTracker.Common.Exceptions;
using WorkTracker.Common.Models;

namespace WorkTracker.Common.Repositories
{
    public abstract class BaseRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected BaseRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        /// <summary>
        /// Get Current user
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        protected User GetCurrentUser() => GetUserFromClaims();

        /// <summary>
        /// Get a User from claims
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        private User GetUserFromClaims()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated != true)
            {
                throw new ApiException(HttpStatusCode.Unauthorized);
            }

            var id = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0";
            var username = user.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? string.Empty;
            var firstName = user.FindFirst("first_name")?.Value ?? string.Empty;
            var lastName = user.FindFirst("last_name")?.Value ?? string.Empty;
            var email = user.FindFirst("email")?.Value ?? string.Empty;

            return new User
            {
                Id = int.TryParse(id, out var userId) ? userId : 0,
                Username = username,
                Password = string.Empty, // Password should not be stored in claims
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };
        }
    }
}
