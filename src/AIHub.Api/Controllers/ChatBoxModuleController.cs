using AIHub.Api.Models;
using AIHub.Modules.ChatBox;
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
    public ActionResult<ApiResponse<ChatReply>> Send([FromBody] ChatMessageRequest request)
    {
        var response = _chatBoxService.Send(request.Message);
        return Ok(ApiResponse.From(response, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
