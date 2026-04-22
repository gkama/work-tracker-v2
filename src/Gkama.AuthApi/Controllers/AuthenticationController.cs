using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Gkama.AuthApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController(IConfiguration configuration) : ControllerBase
{
    [HttpPost("token")]
    public ActionResult<TokenResponse> CreateToken([FromBody] TokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Username and password are required.");
        }

        var expectedUsername = configuration["Auth:Username"] ?? throw new InvalidOperationException("Auth:Username is missing.");
        var expectedPassword = configuration["Auth:Password"] ?? throw new InvalidOperationException("Auth:Password is missing.");
        var suppliedPassword = Encoding.UTF8.GetBytes(request.Password);
        var configuredPassword = Encoding.UTF8.GetBytes(expectedPassword);
        var maxLength = Math.Max(suppliedPassword.Length, configuredPassword.Length);
        var suppliedPadded = new byte[maxLength];
        var configuredPadded = new byte[maxLength];
        suppliedPassword.CopyTo(suppliedPadded, 0);
        configuredPassword.CopyTo(configuredPadded, 0);
        var passwordMatches = CryptographicOperations.FixedTimeEquals(suppliedPadded, configuredPadded) &&
                              suppliedPassword.Length == configuredPassword.Length;

        if (!string.Equals(request.Username, expectedUsername, StringComparison.Ordinal) ||
            !passwordMatches)
        {
            return Unauthorized();
        }

        var issuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");
        var key = configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey is missing.");
        var expirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var value) ? value : 60;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes);

        var tokenDescriptor = new JwtSecurityToken(
            issuer,
            audience,
            [
                new Claim(JwtRegisteredClaimNames.Sub, request.Username),
                new Claim(ClaimTypes.Name, request.Username)
            ],
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        var token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        return Ok(new TokenResponse(token, expiresAt));
    }
}

public sealed record TokenRequest(string Username, string Password);

public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAtUtc);
