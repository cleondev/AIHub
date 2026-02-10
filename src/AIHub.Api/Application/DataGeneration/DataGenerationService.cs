using AIHub.Api.Models;
using AIHub.Api.Services;

namespace AIHub.Api.Application.DataGeneration;

public interface IDataGenerationService
{
    DataGenerationDraft CreateDraft(AiDataGenerationRequest request);

    DataGenerationDraft? GetDraft(Guid id);
}

public sealed class DataGenerationService : IDataGenerationService
{
    private readonly InMemoryStore _store;

    public DataGenerationService(InMemoryStore store)
    {
        _store = store;
    }

    public DataGenerationDraft CreateDraft(AiDataGenerationRequest request)
    {
        var draft = new DataGenerationDraft(
            Id: Guid.NewGuid(),
            Prompt: request.Prompt.Trim(),
            SchemaRef: request.SchemaRef,
            Status: DraftStatus.PendingApproval,
            Payload: request.SeedPayload,
            CreatedAt: DateTimeOffset.UtcNow,
            UpdatedAt: DateTimeOffset.UtcNow);

        _store.DataDrafts[draft.Id] = draft;

        return draft;
    }

    public DataGenerationDraft? GetDraft(Guid id)
    {
        return _store.DataDrafts.TryGetValue(id, out var draft) ? draft : null;
    }
}
