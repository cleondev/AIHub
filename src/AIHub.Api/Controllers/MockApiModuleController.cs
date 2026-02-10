using AIHub.Api.Models;
using AIHub.Modules.MockApi;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("module/mock-api")]
public sealed class MockApiModuleController : ControllerBase
{
    private readonly IMockApiService _mockApiService;

    public MockApiModuleController(IMockApiService mockApiService)
    {
        _mockApiService = mockApiService;
    }

    [HttpGet("query")]
    public ActionResult<ApiResponse<IEnumerable<MockRequest>>> Query([FromQuery] string? keyword)
    {
        return Ok(ApiResponse.From(_mockApiService.Query(keyword), TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("create")]
    public ActionResult<ApiResponse<MockRequest>> Create([FromBody] MockCreateRequest request)
    {
        var created = _mockApiService.Create(request.Name);
        return Ok(ApiResponse.From(created, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("approve/{id:guid}")]
    public ActionResult<ApiResponse<object>> Approve([FromRoute] Guid id)
    {
        var approved = _mockApiService.Approve(id);
        if (approved is null)
        {
            return NotFound(ApiResponse.From(new ErrorResponse("not_found", "Không tìm thấy request."), TraceIdProvider.GetFromHttpContext(HttpContext)));
        }

        return Ok(ApiResponse.From<object>(approved, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
