namespace AIHub.Api.Models;

public sealed record AiDataGenerationRequest(string Prompt, string SchemaRef, string? SeedPayload);

public sealed record DataGenerationDraft(
    Guid Id,
    string Prompt,
    string SchemaRef,
    DraftStatus Status,
    string? Payload,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? ApprovedBy = null,
    string? ApprovalComment = null);

public enum DraftStatus
{
    PendingApproval,
    Approved,
    Rejected
}

public sealed record ApprovalRequest(string Actor, string? Comment);
