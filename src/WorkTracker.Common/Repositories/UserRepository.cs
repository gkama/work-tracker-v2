using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WorkTracker.Common.Constants;
using WorkTracker.Common.Dtos;
using WorkTracker.Common.Exceptions;
using WorkTracker.Common.Interfaces;
using WorkTracker.Common.Models;
using WorkTracker.Common.Services;

namespace WorkTracker.Common.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        private readonly WorkTrackerDbContext _dbContext;
        private readonly ICacheService _cacheService;

        public UserRepository(WorkTrackerDbContext dbContext,
            ICacheService cacheService,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
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
                user = await GetUserAsnyc(username);

                if (user == null)
                {
                    return user;
                }

                await _cacheService.SetAsync(cacheKey, user);
            }

            return user;
        }

        /// <summary>
        /// Get User
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private async Task<User?> GetUserAsnyc(string username) => await _dbContext.Users
            .AsNoTracking()
            .Include(u => u.Organizations)
            .ThenInclude(uo => uo.Organization)
            .ThenInclude(o => o.Projects)
            .ThenInclude(p => p.WorkItems)
            .ThenInclude(wi => wi.WorkItemHours)
            .FirstOrDefaultAsync(x => x.Username == username);

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

        /// <summary>
        /// Create a new organization
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ApiException"></exception>
        public async Task<Organization> CreateOrganizationAsync(Organization model)
        {
            ArgumentNullException.ThrowIfNull(model);

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                throw new ApiException(HttpStatusCode.BadRequest, "Organization name is required.");
            }

            var now = DateTime.UtcNow;
            var currentUser = GetCurrentUser();

            model.Name = model.Name.Trim();
            model.Description = string.IsNullOrWhiteSpace(model.Description)
                ? null
                : model.Description.Trim();
            model.CreatedAt = now;
            model.UpdatedAt = now;

            await _dbContext.Organizations.AddAsync(model);
            await _dbContext.SaveChangesAsync();

            var userOrganization = new UserOrganization
            {
                UserId = currentUser.Id,
                OrganizationId = model.Id,
                CreatedAt = now,
                UpdatedAt = now,
                User = null!,
                Organization = model
            };

            await _dbContext.UserOrganizations.AddAsync(userOrganization);
            await _dbContext.SaveChangesAsync();

            await RefreshUserCacheAsync();

            return model;
        }

        /// <summary>
        /// Refresh User cache
        /// </summary>
        /// <returns></returns>
        private async Task RefreshUserCacheAsync()
        {
            var currentUser = GetCurrentUser();
            var cacheKey = CacheKeys.GetUserKey(currentUser.Username);

            var user = await GetUserAsnyc(currentUser.Username);

            if (user == null)
            {
                return;
            }

            await _cacheService.RemoveAsync(cacheKey);
            await _cacheService.SetAsync(cacheKey, user!);
        }
    }
}