using AIHub.Api.Services;
using AIHub.Modules.ChatBox;
using AIHub.Modules.SemanticKernel;
using Microsoft.AspNetCore.SignalR;

namespace AIHub.Api.Hubs;

public sealed class ChatHub : Hub
{
    private readonly IChatBoxService _chatBoxService;
    private readonly ChatRealtimeStore _chatStore;

    public ChatHub(IChatBoxService chatBoxService, ChatRealtimeStore chatStore)
    {
        _chatBoxService = chatBoxService;
        _chatStore = chatStore;
    }

    public async Task JoinRoom()
    {
        var history = _chatStore.GetRecent();
        await Clients.Caller.SendAsync("chat_history", history);
    }

    public async Task SendMessage(string message)
    {
        var normalized = message?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return;
        }

        var userMessage = _chatStore.Append("Bạn", normalized);
        await Clients.All.SendAsync("chat_message", userMessage);

        var user = new UserContext(
            Context.UserIdentifier ?? Context.ConnectionId,
            Context.User?.Claims
                .Where(claim => string.Equals(claim.Type, "role", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(claim.Type, "roles", StringComparison.OrdinalIgnoreCase))
                .Select(claim => claim.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() is { Length: > 0 } roles
                ? roles
                : ["reader"]);

        var tenant = new TenantContext("default-tenant", "dev");
        var policy = new PolicyContext("v1", false, true);
        var trace = new TraceContext(Context.ConnectionId, Context.ConnectionId);

        var reply = await _chatBoxService.SendAsync(normalized, user, tenant, policy, trace, Context.ConnectionAborted);
        var botMessage = _chatStore.Append("AIHub", reply.Message);
        await Clients.All.SendAsync("chat_message", botMessage);
    }
}
