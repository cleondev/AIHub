using AIHub.Api.Models;
using AIHub.Api.Services;

namespace AIHub.Api.Application.Approval;

public interface IApprovalService
{
    DataGenerationDraft? Approve(Guid id, ApprovalRequest request);

    DataGenerationDraft? Reject(Guid id, ApprovalRequest request);
}

public sealed class ApprovalService : IApprovalService
{
    private readonly InMemoryStore _store;

    public ApprovalService(InMemoryStore store)
    {
        _store = store;
    }

    public DataGenerationDraft? Approve(Guid id, ApprovalRequest request)
    {
        if (!_store.DataDrafts.TryGetValue(id, out var draft))
        {
            return null;
        }

        var approved = draft with
        {
            Status = DraftStatus.Approved,
            ApprovedBy = request.Actor.Trim(),
            ApprovalComment = request.Comment?.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _store.DataDrafts[id] = approved;

        return approved;
    }

    public DataGenerationDraft? Reject(Guid id, ApprovalRequest request)
    {
        if (!_store.DataDrafts.TryGetValue(id, out var draft))
        {
            return null;
        }

        var rejected = draft with
        {
            Status = DraftStatus.Rejected,
            ApprovedBy = request.Actor.Trim(),
            ApprovalComment = request.Comment?.Trim(),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _store.DataDrafts[id] = rejected;

        return rejected;
    }
}
