using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gkama.BusinessApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessController : ControllerBase
{
    [HttpGet("summary")]
    public IActionResult GetSummary()
    {
        var userName = User.Identity?.Name ?? "unknown";
        return Ok(new
        {
            Message = "Business API reached successfully.",
            User = userName
        });
    }
}
