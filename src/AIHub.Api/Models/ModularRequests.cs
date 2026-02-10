namespace AIHub.Api.Models;

public sealed record AddConceptRequest(string Name);

public sealed record AddToolRequest(string Name, string Description);

public sealed record SetLlmConfigRequest(string Provider, string Model, string ApiBaseUrl);

public sealed record ChatMessageRequest(string Message);

public sealed record MockCreateRequest(string Name);
