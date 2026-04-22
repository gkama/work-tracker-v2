using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WorkTracker.Common.Dtos;
using WorkTracker.Common.Extensions;
using WorkTracker.Common.IntegrationEvents;
using WorkTracker.Common.Interfaces;
using WorkTracker.Common.Requests;

namespace WorkTracker.ApiService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly IBackgroundEventPublisher _backgroundEventPublisher;

        public AuthController(IUserRepository userRepository, IAuthService authService, IBackgroundEventPublisher backgroundEventPublisher)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _backgroundEventPublisher = backgroundEventPublisher ?? throw new ArgumentNullException(nameof(backgroundEventPublisher));
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("user")]
        public async Task<IActionResult> AuthUserAsync([FromBody, Required] AuthTokenRequest request)
        {
            var user = await _userRepository.LoginAsync(request.Username, request.Password);

            if (user == null)
            {
                return Unauthorized();
            }

            var token = _authService.GenerateToken(user);

            await _backgroundEventPublisher.QueueAsync(
                new UserLoggedInEvent(user.Id, user.Username)
                {
                    Source = UserLoggedInEvent.EventSource,
                    Type = UserLoggedInEvent.EventType,
                    Data = new UserLoggedInData(user.Id, user.Username, DateTimeOffset.UtcNow)
                },
                UserLoggedInEvent.RoutingKey,
                HttpContext.RequestAborted);

            return new OkObjectResult(token);
        }

        [Authorize]
        [HttpGet]
        [Route("me")]
        public async Task<IActionResult> GetCurrentUserAsync()
        {
            var username = User.Identity?.Name;

            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized();
            }

            var user = await _userRepository.GetAsync(username);

            if (user == null)
            {
                return Unauthorized();
            }

            return new OkObjectResult(user.ToDto());
        }
    }
}
