namespace AIHub.Api.Models;

public sealed record ApiCatalogRequest(string Name, string Description, string Schema);

public sealed record ApiCatalogEntry(Guid Id, string Name, string Description, string Schema, DateTimeOffset CreatedAt);
