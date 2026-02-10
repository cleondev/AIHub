using AIHub.Modules.ChatBox;

namespace AIHub.Api.Services;

public sealed class ExternalChatServiceAdapter : IExternalChatService
{
    private readonly IMinimaxChatService _minimaxChatService;

    public ExternalChatServiceAdapter(IMinimaxChatService minimaxChatService)
    {
        _minimaxChatService = minimaxChatService;
    }

    public async Task<ExternalChatResult?> ReplyAsync(string message, CancellationToken cancellationToken = default)
    {
        var result = await _minimaxChatService.TrySendAsync(message, cancellationToken);
        return result is null ? null : new ExternalChatResult(result.Message, result.ToolCalls);
    }
}
