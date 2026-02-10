namespace AIHub.Modules.SemanticKernel;

public sealed record UserContext(string UserId, IReadOnlyList<string> Roles);

public sealed record TenantContext(string TenantId, string Environment);

public sealed record PolicyContext(string PolicyVersion, bool RequireCitation, bool AllowExternalModel);

public sealed record TraceContext(string SessionId, string CorrelationId);

public sealed record AIRequestEnvelope(
    string Message,
    UserContext User,
    TenantContext Tenant,
    PolicyContext Policy,
    TraceContext Trace,
    DateTimeOffset RequestedAt);

public sealed record ToolCallTrace(string ToolName, object Parameters, object? Result, bool Success, string? Error = null);

public sealed record KernelExecutionResult(
    string Message,
    object? Data,
    string Source,
    IReadOnlyList<string> Citations,
    IReadOnlyList<ToolCallTrace> ToolCalls);

public sealed record PreExecutionContext(AIRequestEnvelope Envelope, string Intent, bool IsAllowed);

public interface IRequestPolicyGuard
{
    PreExecutionContext Validate(AIRequestEnvelope envelope);
}

public interface IResponsePolicyGuard
{
    KernelExecutionResult Apply(AIRequestEnvelope envelope, KernelExecutionResult result);
}

public interface ISemanticKernelPlugin
{
    string Name { get; }

    bool CanHandle(string intent, string message);

    Task<KernelExecutionResult?> ExecuteAsync(AIRequestEnvelope envelope, string intent, CancellationToken cancellationToken = default);
}

public interface ISemanticKernelRuntime
{
    Task<KernelExecutionResult> ExecuteAsync(AIRequestEnvelope envelope, CancellationToken cancellationToken = default);
}

public sealed class SemanticKernelRuntime : ISemanticKernelRuntime
{
    private readonly IReadOnlyList<ISemanticKernelPlugin> _plugins;
    private readonly IRequestPolicyGuard _requestPolicyGuard;
    private readonly IResponsePolicyGuard _responsePolicyGuard;

    public SemanticKernelRuntime(
        IEnumerable<ISemanticKernelPlugin> plugins,
        IRequestPolicyGuard requestPolicyGuard,
        IResponsePolicyGuard responsePolicyGuard)
    {
        _plugins = plugins.ToList();
        _requestPolicyGuard = requestPolicyGuard;
        _responsePolicyGuard = responsePolicyGuard;
    }

    public async Task<KernelExecutionResult> ExecuteAsync(AIRequestEnvelope envelope, CancellationToken cancellationToken = default)
    {
        var intent = ClassifyIntent(envelope.Message);
        if (envelope.Policy.AllowExternalModel && !IsWorkflowIntent(intent))
        {
            intent = "GeneralChat";
        }

        var preExecution = _requestPolicyGuard.Validate(envelope with { Message = envelope.Message.Trim() }) with { Intent = intent };
        if (!preExecution.IsAllowed)
        {
            return new KernelExecutionResult(
                "Yêu cầu bị từ chối bởi policy hiện tại.",
                null,
                "policy",
                [],
                []);
        }

        foreach (var plugin in _plugins)
        {
            if (!plugin.CanHandle(intent, envelope.Message))
            {
                continue;
            }

            var result = await plugin.ExecuteAsync(envelope, intent, cancellationToken);
            if (result is null)
            {
                continue;
            }

            return _responsePolicyGuard.Apply(envelope, result);
        }

        var fallback = new KernelExecutionResult(
            "Tôi chưa hiểu yêu cầu. Gợi ý: 'liệt kê abc', 'tạo def', hoặc 'approve <request-id>'.",
            null,
            "fallback",
            [],
            []);

        return _responsePolicyGuard.Apply(envelope, fallback);
    }

    private static string ClassifyIntent(string message)
    {
        var normalized = message.Trim();

        if (normalized.StartsWith("approve ", StringComparison.OrdinalIgnoreCase))
        {
            return "Approval";
        }

        if (normalized.StartsWith("tạo ", StringComparison.OrdinalIgnoreCase))
        {
            return "CreateRequest";
        }

        if (normalized.Contains("liệt kê", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("danh sách", StringComparison.OrdinalIgnoreCase))
        {
            return "ListKnowledge";
        }

        return "GeneralChat";
    }

    private static bool IsWorkflowIntent(string intent)
    {
        return string.Equals(intent, "CreateRequest", StringComparison.OrdinalIgnoreCase)
               || string.Equals(intent, "Approval", StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class DefaultRequestPolicyGuard : IRequestPolicyGuard
{
    public PreExecutionContext Validate(AIRequestEnvelope envelope)
    {
        var normalized = envelope.Message.Trim();
        var isAllowed = !string.IsNullOrWhiteSpace(normalized);

        if (envelope.Policy.AllowExternalModel)
        {
            return new PreExecutionContext(envelope, "GeneralChat", isAllowed);
        }

        if (normalized.StartsWith("tạo ", StringComparison.OrdinalIgnoreCase)
            && !envelope.User.Roles.Contains("writer", StringComparer.OrdinalIgnoreCase)
            && !envelope.User.Roles.Contains("admin", StringComparer.OrdinalIgnoreCase))
        {
            return new PreExecutionContext(envelope, "CreateRequest", false);
        }

        if (normalized.StartsWith("approve ", StringComparison.OrdinalIgnoreCase)
            && !envelope.User.Roles.Contains("approver", StringComparer.OrdinalIgnoreCase)
            && !envelope.User.Roles.Contains("admin", StringComparer.OrdinalIgnoreCase))
        {
            return new PreExecutionContext(envelope, "Approval", false);
        }

        return new PreExecutionContext(envelope, "GeneralChat", isAllowed);
    }
}

public sealed class DefaultResponsePolicyGuard : IResponsePolicyGuard
{
    public KernelExecutionResult Apply(AIRequestEnvelope envelope, KernelExecutionResult result)
    {
        if (!envelope.Policy.RequireCitation || result.Citations.Count > 0)
        {
            return result;
        }

        var policyCitation = $"policy:{envelope.Policy.PolicyVersion}";
        return result with
        {
            Citations = [..result.Citations, policyCitation]
        };
    }
}
