using AIHub.Api.Models;
using AIHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("approval")]
public sealed class ApprovalController : ControllerBase
{
    private readonly InMemoryStore _store;

    public ApprovalController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpPost("{id:guid}/approve")]
    public ActionResult<ApiResponse<DataGenerationDraft>> Approve([FromRoute] Guid id, [FromBody] ApprovalRequest request)
    {
        if (!_store.DataDrafts.TryGetValue(id, out var draft))
        {
            var error = new ErrorResponse("draft_not_found", "Không tìm thấy bản nháp.");
            return NotFound(ApiResponse.From(error, TraceIdProvider.GetFromHttpContext(HttpContext)));
        }

        var approved = draft with
        {
            Status = DraftStatus.Approved,
            ApprovedBy = request.Actor.Trim(),
            ApprovalComment = request.Comment?.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _store.DataDrafts[id] = approved;

        return Ok(ApiResponse.From(approved, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("{id:guid}/reject")]
    public ActionResult<ApiResponse<DataGenerationDraft>> Reject([FromRoute] Guid id, [FromBody] ApprovalRequest request)
    {
        if (!_store.DataDrafts.TryGetValue(id, out var draft))
        {
            var error = new ErrorResponse("draft_not_found", "Không tìm thấy bản nháp.");
            return NotFound(ApiResponse.From(error, TraceIdProvider.GetFromHttpContext(HttpContext)));
        }

        var rejected = draft with
        {
            Status = DraftStatus.Rejected,
            ApprovedBy = request.Actor.Trim(),
            ApprovalComment = request.Comment?.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _store.DataDrafts[id] = rejected;

        return Ok(ApiResponse.From(rejected, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
