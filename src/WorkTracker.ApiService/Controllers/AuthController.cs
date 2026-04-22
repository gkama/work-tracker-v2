using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
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

        public AuthController(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
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

            return new OkObjectResult(token);
        }
    }
}
