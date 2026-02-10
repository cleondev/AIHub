using AIHub.Modules.Management;
using AIHub.Modules.Tooling;

namespace AIHub.Modules.ChatBox;

public sealed record ChatReply(string Message, object? Data = null);

public interface IChatBoxService
{
    ChatReply Send(string message);
}

public sealed class ChatBoxService : IChatBoxService
{
    private readonly IManagementService _managementService;
    private readonly IToolGateway _toolGateway;

    public ChatBoxService(IManagementService managementService, IToolGateway toolGateway)
    {
        _managementService = managementService;
        _toolGateway = toolGateway;
    }

    public ChatReply Send(string message)
    {
        var normalized = message.Trim();

        if (normalized.Contains("liệt kê", StringComparison.OrdinalIgnoreCase) ||
            normalized.Contains("danh sách", StringComparison.OrdinalIgnoreCase))
        {
            var concepts = _managementService.GetConcepts().ToList();
            return new ChatReply($"Đã tìm thấy {concepts.Count} khái niệm.", concepts);
        }

        if (normalized.StartsWith("tạo ", StringComparison.OrdinalIgnoreCase))
        {
            var conceptName = normalized[4..].Trim();
            var draft = _toolGateway.WriteCreateRequest(conceptName);
            return new ChatReply($"Đã tạo request cho '{conceptName}' với trạng thái {draft.Status}.", draft);
        }

        if (normalized.StartsWith("approve ", StringComparison.OrdinalIgnoreCase))
        {
            var idText = normalized[8..].Trim();
            if (!Guid.TryParse(idText, out var requestId))
            {
                return new ChatReply("ID approve không hợp lệ.");
            }

            var approved = _toolGateway.WriteApproveRequest(requestId);
            return approved is null
                ? new ChatReply("Không tìm thấy request cần approve.")
                : new ChatReply($"Request {requestId} đã được chuyển sang {approved.Status}.", approved);
        }

        var matched = _toolGateway.ReadRequests(normalized).ToList();
        if (matched.Count > 0)
        {
            return new ChatReply($"Đã query API và thấy {matched.Count} bản ghi liên quan '{normalized}'.", matched);
        }

        return new ChatReply("Tôi chưa hiểu yêu cầu. Gợi ý: 'liệt kê abc', 'tạo def', hoặc 'approve <request-id>'.");
    }
}
