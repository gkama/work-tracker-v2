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

        /// <summary>
        /// Get User by username
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<User?> GetAsync(string username)
        {
            User? user;
            var cacheKey = CacheKeys.GetUserKey(username);

            user = await _cacheService.GetAsync<User>(cacheKey);

            if (user == null)
            {
                user = await _dbContext.Users
                        .AsNoTracking()
                        .Include(u => u.Organizations)
                            .ThenInclude(uo => uo.Organization)
                                .ThenInclude(o => o.Projects)
                                    .ThenInclude(p => p.WorkItems)
                                        .ThenInclude(wi => wi.WorkItemHours)
                        .FirstOrDefaultAsync(x => x.Username == username);

                if (user == null)
                {
                    return user;
                }

                await _cacheService.SetAsync(cacheKey, user);

            }

            return user;
        }

        /// <summary>
        /// Login User
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task<User> LoginAsync(string? username, string? password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
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
