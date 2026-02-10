using AIHub.Api.Models;
using AIHub.Modules.MockApi;
using AIHub.Modules.Tooling;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("module/tool")]
public sealed class ToolModuleController : ControllerBase
{
    private readonly IToolGateway _toolGateway;

    public ToolModuleController(IToolGateway toolGateway)
    {
        _toolGateway = toolGateway;
    }

    [HttpGet("read")]
    public ActionResult<ApiResponse<IEnumerable<MockRequest>>> Read([FromQuery] string? keyword)
    {
        return Ok(ApiResponse.From(_toolGateway.ReadRequests(keyword), TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("write/create")]
    public ActionResult<ApiResponse<MockRequest>> Create([FromBody] MockCreateRequest request)
    {
        var created = _toolGateway.WriteCreateRequest(request.Name);
        return Ok(ApiResponse.From(created, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("write/approve/{id:guid}")]
    public ActionResult<ApiResponse<object>> Approve([FromRoute] Guid id)
    {
        var approved = _toolGateway.WriteApproveRequest(id);
        if (approved is null)
        {
            return NotFound(ApiResponse.From(new ErrorResponse("not_found", "Không tìm thấy request."), TraceIdProvider.GetFromHttpContext(HttpContext)));
        }

        return Ok(ApiResponse.From<object>(approved, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
