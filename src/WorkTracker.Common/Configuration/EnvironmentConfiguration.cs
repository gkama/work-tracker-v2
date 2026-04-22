using Microsoft.Extensions.Options;
using WorkTracker.Common.Interfaces;

namespace WorkTracker.Common.Configuration
{
    public class EnvironmentConfiguration : IEnvironmentConfiguration
    {
        public JwtConfiguration JwtConfiguration { get; set; }

        public EnvironmentConfiguration(IOptions<JwtConfiguration> jwtConfiguration)
        {
            JwtConfiguration = jwtConfiguration.Value;
        }
    }

    public class JwtConfiguration
    {
        public const string Section = "Jwt";

        public required string Key { get; set; }
        public required string Issuer { get; set; }
        public required string Audience { get; set; }
        public int ExpiresInMinutes { get; set; }
    }
}