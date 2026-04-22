using Microsoft.EntityFrameworkCore;
using System.Net;
using WorkTracker.Common.Constants;
using WorkTracker.Common.Exceptions;
using WorkTracker.Common.Interfaces;
using WorkTracker.Common.Models;
using WorkTracker.Common.Services;

namespace WorkTracker.Common.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WorkTrackerDbContext _dbContext;
        private readonly ICacheService _cacheService;

        public UserRepository(WorkTrackerDbContext dbContext,
            ICacheService cacheService)
        {
            _dbContext = dbContext;
            _cacheService = cacheService;
        }

        public Task<User?> GetAsync(string username) =>
            _cacheService.GetOrSetNullableAsync(
                CacheKeys.GetUserKey(username),
                () => _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Username == username));

        public async Task<User> LoginAsync(string? username, string? password)
        {
            if (string.IsNullOrEmpty(username)
                || string.IsNullOrEmpty(password))
            {
                throw new ApiException(HttpStatusCode.Unauthorized);
            }

            var user = await GetAsync(username)
                ?? throw new ApiException(HttpStatusCode.NotFound);

            var isValidPassword = EncryptionService.Verify(password, user.Password);

            if (!isValidPassword)
            {
                throw new ApiException(HttpStatusCode.Unauthorized);
            }

            return user;
        }
    }
}
