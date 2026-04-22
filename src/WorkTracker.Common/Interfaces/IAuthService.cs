using System.Security.Claims;
using WorkTracker.Common.Models;
using WorkTracker.Common.Responses;

namespace WorkTracker.Common.Interfaces
{
    public interface IAuthService
    {
        AuthTokenResponse GenerateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
