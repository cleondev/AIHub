using AIHub.Api.Models;
using AIHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("knowledge")]
public sealed class KnowledgeController : ControllerBase
{
    private readonly InMemoryStore _store;

    public KnowledgeController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpPost("documents")]
    public ActionResult<ApiResponse<Document>> UploadDocument([FromBody] DocumentUploadRequest request)
    {
        var document = new Document(
            Id: Guid.NewGuid(),
            Title: request.Title.Trim(),
            SourceType: request.SourceType.Trim(),
            Status: "ingested",
            CreatedAt: DateTimeOffset.UtcNow);

        _store.Documents[document.Id] = document;

        return Ok(ApiResponse.From(document, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet("documents")]
    public ActionResult<ApiResponse<IEnumerable<Document>>> GetDocuments()
    {
        var documents = _store.Documents.Values.OrderByDescending(item => item.CreatedAt);
        return Ok(ApiResponse.From(documents, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("query")]
    public ActionResult<ApiResponse<KnowledgeQueryResponse>> Query([FromBody] KnowledgeQueryRequest request)
    {
        var response = new KnowledgeQueryResponse(
            Query: request.Query,
            Answer: "Câu trả lời mẫu từ hệ thống AIHub (MVP).",
            Sources:
            [
                new KnowledgeSource("kb:sample-doc", "Sample Document", 0.82m),
                new KnowledgeSource("kb:glossary", "Glossary", 0.76m)
            ]);

        return Ok(ApiResponse.From(response, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
