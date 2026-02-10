using AIHub.Api.Services;
using AIHub.Modules.ChatBox;
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

        var reply = await _chatBoxService.SendAsync(normalized);
        var botMessage = _chatStore.Append("AIHub", reply.Message);
        await Clients.All.SendAsync("chat_message", botMessage);
    }
}
