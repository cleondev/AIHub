using AIHub.Api.Models;
using AIHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("glossary")]
public sealed class GlossaryController : ControllerBase
{
    private readonly InMemoryStore _store;

    public GlossaryController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpPost("terms")]
    public ActionResult<ApiResponse<GlossaryTerm>> CreateTerm([FromBody] GlossaryTermRequest request)
    {
        var term = new GlossaryTerm(
            Id: Guid.NewGuid(),
            Term: request.Term.Trim(),
            Definition: request.Definition.Trim(),
            CreatedAt: DateTimeOffset.UtcNow);

        _store.GlossaryTerms[term.Id] = term;

        return Ok(ApiResponse.From(term, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet("terms")]
    public ActionResult<ApiResponse<IEnumerable<GlossaryTerm>>> GetTerms()
    {
        var terms = _store.GlossaryTerms.Values.OrderBy(term => term.Term);
        return Ok(ApiResponse.From(terms, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
