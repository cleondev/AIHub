using AIHub.Api.Models;
using AIHub.Api.Services;

namespace AIHub.Api.Application.Knowledge;

public interface IKnowledgeService
{
    Document UploadDocument(DocumentUploadRequest request);

    IEnumerable<Document> GetDocuments();

    KnowledgeQueryResponse Query(KnowledgeQueryRequest request);
}

public sealed class KnowledgeService : IKnowledgeService
{
    private readonly InMemoryStore _store;

    public KnowledgeService(InMemoryStore store)
    {
        _store = store;
    }

    public Document UploadDocument(DocumentUploadRequest request)
    {
        var document = new Document(
            Id: Guid.NewGuid(),
            Title: request.Title.Trim(),
            SourceType: request.SourceType.Trim(),
            Status: "ingested",
            CreatedAt: DateTimeOffset.UtcNow);

        _store.Documents[document.Id] = document;

        return document;
    }

    public IEnumerable<Document> GetDocuments()
    {
        return _store.Documents.Values.OrderByDescending(item => item.CreatedAt);
    }

    public KnowledgeQueryResponse Query(KnowledgeQueryRequest request)
    {
        return new KnowledgeQueryResponse(
            Query: request.Query,
            Answer: "Câu trả lời mẫu từ hệ thống AIHub (MVP).",
            Sources:
            [
                new KnowledgeSource("kb:sample-doc", "Sample Document", 0.82m),
                new KnowledgeSource("kb:glossary", "Glossary", 0.76m)
            ]);
    }
}
