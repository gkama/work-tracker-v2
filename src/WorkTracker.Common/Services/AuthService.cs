using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using WorkTracker.Common.Exceptions;
using WorkTracker.Common.Interfaces;
using WorkTracker.Common.Models;
using WorkTracker.Common.Responses;

namespace WorkTracker.Common.Services
{
    public class AuthService : IAuthService
    {
        private readonly IEnvironmentConfiguration _environmentConfiguration;
        private readonly JwtSecurityTokenHandler _tokenHandler;

        private readonly string _jwtKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpiresInMinutes;

        public AuthService(IEnvironmentConfiguration environmentConfiguration)
        {
            _environmentConfiguration = environmentConfiguration;
            _tokenHandler = new();

            _jwtKey = _environmentConfiguration.JwtConfiguration.Key;
            _jwtIssuer = _environmentConfiguration.JwtConfiguration.Issuer;
            _jwtAudience = _environmentConfiguration.JwtConfiguration.Audience;
            _jwtExpiresInMinutes = _environmentConfiguration.JwtConfiguration.ExpiresInMinutes;
        }

        public AuthTokenResponse GenerateToken(User user)
        {

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.UniqueName, user.Username),
                new("first_name", user.FirstName ?? ""),
                new("last_name", user.LastName ?? ""),
                new("email", user.Email ?? ""),
                new("username", user.Username ?? ""),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var now = DateTime.UtcNow;
            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: now.AddMinutes(_jwtExpiresInMinutes),
                signingCredentials: credentials);

            return new AuthTokenResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                TokenType = "Bearer",
                ExpiresIn = (int)(token.ValidTo - now).TotalSeconds + 1
            };
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtIssuer,
                ValidateAudience = true,
                ValidAudience = _jwtAudience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2),
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey))
            };

            try
            {
                var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                // Optional: ensure token is a JWT and uses expected algorithm
                if (validatedToken is JwtSecurityToken jwt &&
                    jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                throw new ApiException(HttpStatusCode.Unauthorized);
            }
            catch
            {
                throw new ApiException(HttpStatusCode.Unauthorized);
            }
        }
    }
}