using AIHub.Api.Application.DataGeneration;
using AIHub.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("ai/data-generation")]
public sealed class AiDataGenerationController : ControllerBase
{
    private readonly IDataGenerationService _dataGenerationService;

    public AiDataGenerationController(IDataGenerationService dataGenerationService)
    {
        _dataGenerationService = dataGenerationService;
    }

    [HttpPost]
    public ActionResult<ApiResponse<DataGenerationDraft>> CreateDraft([FromBody] AiDataGenerationRequest request)
    {
        var draft = _dataGenerationService.CreateDraft(request);
        return Ok(ApiResponse.From(draft, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ApiResponse<DataGenerationDraft>> GetDraft([FromRoute] Guid id)
    {
        var draft = _dataGenerationService.GetDraft(id);
        if (draft is null)
        {
            var error = new ErrorResponse("draft_not_found", "Không tìm thấy bản nháp.");
            return NotFound(ApiResponse.From(error, TraceIdProvider.GetFromHttpContext(HttpContext)));
        }

        return Ok(ApiResponse.From(draft, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
