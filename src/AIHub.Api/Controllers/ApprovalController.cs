using AIHub.Api.Application.Approval;
using AIHub.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("approval")]
public sealed class ApprovalController : ControllerBase
{
    private readonly IApprovalService _approvalService;

    public ApprovalController(IApprovalService approvalService)
    {
        _approvalService = approvalService;
    }

    [HttpPost("{id:guid}/approve")]
    public ActionResult<ApiResponse<DataGenerationDraft>> Approve([FromRoute] Guid id, [FromBody] ApprovalRequest request)
    {
        var approved = _approvalService.Approve(id, request);
        if (approved is null)
        {
            var error = new ErrorResponse("draft_not_found", "Không tìm thấy bản nháp.");
            return NotFound(ApiResponse.From(error, TraceIdProvider.GetFromHttpContext(HttpContext)));
        }

        return Ok(ApiResponse.From(approved, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("{id:guid}/reject")]
    public ActionResult<ApiResponse<DataGenerationDraft>> Reject([FromRoute] Guid id, [FromBody] ApprovalRequest request)
    {
        var rejected = _approvalService.Reject(id, request);
        if (rejected is null)
        {
            var error = new ErrorResponse("draft_not_found", "Không tìm thấy bản nháp.");
            return NotFound(ApiResponse.From(error, TraceIdProvider.GetFromHttpContext(HttpContext)));
        }

        return Ok(ApiResponse.From(rejected, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
