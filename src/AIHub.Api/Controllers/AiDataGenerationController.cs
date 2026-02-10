using AIHub.Api.Models;
using AIHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("ai/data-generation")]
public sealed class AiDataGenerationController : ControllerBase
{
    private readonly InMemoryStore _store;

    public AiDataGenerationController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpPost]
    public ActionResult<ApiResponse<DataGenerationDraft>> CreateDraft([FromBody] AiDataGenerationRequest request)
    {
        var draft = new DataGenerationDraft(
            Id: Guid.NewGuid(),
            Prompt: request.Prompt.Trim(),
            SchemaRef: request.SchemaRef,
            Status: DraftStatus.PendingApproval,
            Payload: request.SeedPayload,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);

        _store.DataDrafts[draft.Id] = draft;

        return Ok(ApiResponse.From(draft, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<ApiResponse<DataGenerationDraft>> GetDraft([FromRoute] Guid id)
    {
        if (!_store.DataDrafts.TryGetValue(id, out var draft))
        {
            var error = new ErrorResponse("draft_not_found", "Không tìm thấy bản nháp.");
            return NotFound(ApiResponse.From(error, TraceIdProvider.GetFromHttpContext(HttpContext)));
        }

        return Ok(ApiResponse.From(draft, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
