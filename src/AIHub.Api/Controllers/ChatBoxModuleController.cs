using AIHub.Api.Models;
using AIHub.Modules.ChatBox;
using AIHub.Modules.SemanticKernel;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("module/chatbox")]
public sealed class ChatBoxModuleController : ControllerBase
{
    private readonly IChatBoxService _chatBoxService;

    public ChatBoxModuleController(IChatBoxService chatBoxService)
    {
        _chatBoxService = chatBoxService;
    }

    [HttpPost("message")]
    public async Task<ActionResult<ApiResponse<ChatReply>>> Send([FromBody] ChatMessageRequest request)
    {
        var user = new UserContext(
            request.UserId ?? "anonymous",
            request.Roles is { Count: > 0 } ? request.Roles : ["reader"]);

        var tenant = new TenantContext(
            request.TenantId ?? "default-tenant",
            request.Environment ?? "dev");

        var policy = new PolicyContext(
            "v1",
            request.RequireCitation,
            request.AllowExternalModel);

        var trace = new TraceContext(
            TraceIdProvider.GetFromHttpContext(HttpContext),
            HttpContext.TraceIdentifier);

        var response = await _chatBoxService.SendAsync(request.Message, user, tenant, policy, trace, HttpContext.RequestAborted);
        return Ok(ApiResponse.From(response, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
