namespace AIHub.Api.Models;

public sealed record PolicyRequest(string Name, string Description, IReadOnlyList<string> Rules);

public sealed record PolicyDefinition(Guid Id, string Name, string Description, IReadOnlyList<string> Rules, DateTimeOffset CreatedAt);
