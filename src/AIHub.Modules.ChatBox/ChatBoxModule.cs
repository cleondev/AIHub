using AIHub.Modules.Management;
using AIHub.Modules.SemanticKernel;
using AIHub.Modules.Tooling;

namespace AIHub.Modules.ChatBox;

public sealed record ChatReply(
    string Message,
    object? Data = null,
    string Source = "rule",
    IReadOnlyList<string>? Citations = null,
    IReadOnlyList<ToolCallTrace>? ToolCalls = null);

public sealed record ExternalChatResult(string Message, IReadOnlyList<ToolCallTrace> ToolCalls);

public interface IExternalChatService
{
    Task<ExternalChatResult?> ReplyAsync(string message, CancellationToken cancellationToken = default);
}

public interface IChatBoxService
{
    Task<ChatReply> SendAsync(
        string message,
        UserContext user,
        TenantContext tenant,
        PolicyContext policy,
        TraceContext trace,
        CancellationToken cancellationToken = default);
}

public sealed class ChatBoxService : IChatBoxService
{
    private readonly ISemanticKernelRuntime _semanticKernelRuntime;

    public ChatBoxService(ISemanticKernelRuntime semanticKernelRuntime)
    {
        _semanticKernelRuntime = semanticKernelRuntime;
    }

    public async Task<ChatReply> SendAsync(
        string message,
        UserContext user,
        TenantContext tenant,
        PolicyContext policy,
        TraceContext trace,
        CancellationToken cancellationToken = default)
    {
        var envelope = new AIRequestEnvelope(message, user, tenant, policy, trace, DateTimeOffset.UtcNow);
        var result = await _semanticKernelRuntime.ExecuteAsync(envelope, cancellationToken);

        return new ChatReply(result.Message, result.Data, result.Source, result.Citations, result.ToolCalls);
    }
}

public sealed class KnowledgePlugin : ISemanticKernelPlugin
{
    private readonly IManagementService _managementService;

    public KnowledgePlugin(IManagementService managementService)
    {
        _managementService = managementService;
    }

    public string Name => "KnowledgePlugin";

    public bool CanHandle(string intent, string message) => string.Equals(intent, "ListKnowledge", StringComparison.OrdinalIgnoreCase);

    public Task<KernelExecutionResult?> ExecuteAsync(AIRequestEnvelope envelope, string intent, CancellationToken cancellationToken = default)
    {
        var concepts = _managementService.GetConcepts().ToList();
        KernelExecutionResult result = new(
            $"Đã tìm thấy {concepts.Count} khái niệm.",
            concepts,
            "plugin:knowledge",
            ["knowledge:concepts"],
            [new ToolCallTrace("ManagementService.GetConcepts", new { envelope.Tenant.TenantId }, concepts, true)]);

        return Task.FromResult<KernelExecutionResult?>(result);
    }
}

public sealed class RequestWorkflowPlugin : ISemanticKernelPlugin
{
    private readonly IToolGateway _toolGateway;

    public RequestWorkflowPlugin(IToolGateway toolGateway)
    {
        _toolGateway = toolGateway;
    }

    public string Name => "RequestWorkflowPlugin";

    public bool CanHandle(string intent, string message)
    {
        return string.Equals(intent, "CreateRequest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(intent, "Approval", StringComparison.OrdinalIgnoreCase)
               || string.Equals(intent, "GeneralChat", StringComparison.OrdinalIgnoreCase);
    }

    public Task<KernelExecutionResult?> ExecuteAsync(AIRequestEnvelope envelope, string intent, CancellationToken cancellationToken = default)
    {
        var normalized = envelope.Message.Trim();

        if (string.Equals(intent, "CreateRequest", StringComparison.OrdinalIgnoreCase))
        {
            var conceptName = normalized[4..].Trim();
            var draft = _toolGateway.WriteCreateRequest(conceptName);
            KernelExecutionResult createResult = new(
                $"Đã tạo request cho '{conceptName}' với trạng thái {draft.Status}.",
                draft,
                "plugin:workflow",
                ["mock-api:request-create"],
                [new ToolCallTrace("ToolGateway.WriteCreateRequest", new { conceptName }, draft, true)]);

            return Task.FromResult<KernelExecutionResult?>(createResult);
        }

        if (string.Equals(intent, "Approval", StringComparison.OrdinalIgnoreCase))
        {
            var idText = normalized[8..].Trim();
            if (!Guid.TryParse(idText, out var requestId))
            {
                KernelExecutionResult invalidResult = new(
                    "ID approve không hợp lệ.",
                    null,
                    "plugin:workflow",
                    [],
                    [new ToolCallTrace("ToolGateway.WriteApproveRequest", new { idText }, null, false, "invalid-guid")]);

                return Task.FromResult<KernelExecutionResult?>(invalidResult);
            }

            var approved = _toolGateway.WriteApproveRequest(requestId);
            var success = approved is not null;
            KernelExecutionResult approvalResult = success
                ? new KernelExecutionResult(
                    $"Request {requestId} đã được chuyển sang {approved!.Status}.",
                    approved,
                    "plugin:workflow",
                    ["mock-api:request-approve"],
                    [new ToolCallTrace("ToolGateway.WriteApproveRequest", new { requestId }, approved, true)])
                : new KernelExecutionResult(
                    "Không tìm thấy request cần approve.",
                    null,
                    "plugin:workflow",
                    [],
                    [new ToolCallTrace("ToolGateway.WriteApproveRequest", new { requestId }, null, false, "not-found")]);

            return Task.FromResult<KernelExecutionResult?>(approvalResult);
        }

        var matched = _toolGateway.ReadRequests(normalized).ToList();
        if (matched.Count == 0)
        {
            return Task.FromResult<KernelExecutionResult?>(null);
        }

        KernelExecutionResult queryResult = new(
            $"Đã query API và thấy {matched.Count} bản ghi liên quan '{normalized}'.",
            matched,
            "plugin:workflow",
            ["mock-api:request-query"],
            [new ToolCallTrace("ToolGateway.ReadRequests", new { keyword = normalized }, matched, true)]);

        return Task.FromResult<KernelExecutionResult?>(queryResult);
    }
}

public sealed class ExternalChatPlugin : ISemanticKernelPlugin
{
    private readonly IExternalChatService _externalChatService;

    public ExternalChatPlugin(IExternalChatService externalChatService)
    {
        _externalChatService = externalChatService;
    }

    public string Name => "ExternalChatPlugin";

    public bool CanHandle(string intent, string message) => string.Equals(intent, "GeneralChat", StringComparison.OrdinalIgnoreCase);

    public async Task<KernelExecutionResult?> ExecuteAsync(AIRequestEnvelope envelope, string intent, CancellationToken cancellationToken = default)
    {
        if (!envelope.Policy.AllowExternalModel)
        {
            return null;
        }

        var externalReply = await _externalChatService.ReplyAsync(envelope.Message.Trim(), cancellationToken);
        if (externalReply is null || string.IsNullOrWhiteSpace(externalReply.Message))
        {
            return null;
        }

        var toolCalls = new List<ToolCallTrace>
        {
            new("ExternalChatService.ReplyAsync", new { message = envelope.Message }, "<text>", true)
        };
        toolCalls.AddRange(externalReply.ToolCalls);

        return new KernelExecutionResult(
            externalReply.Message,
            null,
            "model:minimax",
            [],
            toolCalls);
    }
}
