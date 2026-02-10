using AIHub.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<object>> GetHealth()
    {
        var payload = new { status = "ok" };
        return Ok(ApiResponse.From(payload, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
