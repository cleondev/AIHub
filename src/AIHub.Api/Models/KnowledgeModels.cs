namespace AIHub.Api.Models;

public sealed record DocumentUploadRequest(string Title, string SourceType, string? Content);

public sealed record Document(Guid Id, string Title, string SourceType, string Status, DateTimeOffset CreatedAt);

public sealed record KnowledgeQueryRequest(string Query, int TopK = 5);

public sealed record KnowledgeQueryResponse(string Query, string Answer, IReadOnlyList<KnowledgeSource> Sources);

public sealed record KnowledgeSource(string Id, string Title, decimal Score);
