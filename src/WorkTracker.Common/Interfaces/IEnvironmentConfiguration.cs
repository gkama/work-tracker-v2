using WorkTracker.Common.Configuration;

namespace WorkTracker.Common.Interfaces
{
    public interface IEnvironmentConfiguration
    {
        JwtConfiguration JwtConfiguration { get; set; }
    }
}
