namespace AIHub.Api.Models;

public sealed record AddConceptRequest(string Name);

public sealed record AddToolRequest(string Name, string Description);

public sealed record SetLlmConfigRequest(string Provider, string Model, string ApiBaseUrl, string? ApiKey, string? GroupId);

public sealed record ChatMessageRequest(
    string Message,
    string? UserId = null,
    IReadOnlyList<string>? Roles = null,
    string? TenantId = null,
    string? Environment = null,
    bool RequireCitation = true,
    bool AllowExternalModel = true);

public sealed record MockCreateRequest(string Name);

public sealed record PurchaseCreateRequest(Guid ProductId, int Quantity);
