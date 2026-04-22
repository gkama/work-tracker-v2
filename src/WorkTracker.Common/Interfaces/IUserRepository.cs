using WorkTracker.Common.Models;

namespace WorkTracker.Common.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetAsync(string username);
        Task<User> LoginAsync(string? username, string? password);
    }
}
