using AIHub.Api.Application.Knowledge;
using AIHub.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("knowledge")]
public sealed class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeService _knowledgeService;

    public KnowledgeController(IKnowledgeService knowledgeService)
    {
        _knowledgeService = knowledgeService;
    }

    [HttpPost("documents")]
    public ActionResult<ApiResponse<Document>> UploadDocument([FromBody] DocumentUploadRequest request)
    {
        var document = _knowledgeService.UploadDocument(request);
        return Ok(ApiResponse.From(document, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet("documents")]
    public ActionResult<ApiResponse<IEnumerable<Document>>> GetDocuments()
    {
        var documents = _knowledgeService.GetDocuments();
        return Ok(ApiResponse.From(documents, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("query")]
    public ActionResult<ApiResponse<KnowledgeQueryResponse>> Query([FromBody] KnowledgeQueryRequest request)
    {
        var response = _knowledgeService.Query(request);
        return Ok(ApiResponse.From(response, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
