using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WorkTracker.Common.Dtos;
using WorkTracker.Common.Extensions;
using WorkTracker.Common.Interfaces;

namespace WorkTracker.ApiService.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public OrganizationController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrganizationAsync([FromBody, Required] OrganizationDto dto)
        {
            var organization = dto.ToModel();

            organization = await _userRepository.CreateOrganizationAsync(organization);

            return new OkObjectResult(organization.ToDto());
        }
    }
}
