namespace AIHub.Api.Models;

public sealed record GlossaryTermRequest(string Term, string Definition);

public sealed record GlossaryTerm(Guid Id, string Term, string Definition, DateTimeOffset CreatedAt);
