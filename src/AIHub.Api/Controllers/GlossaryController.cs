using AIHub.Api.Application.Glossary;
using AIHub.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("glossary")]
public sealed class GlossaryController : ControllerBase
{
    private readonly IGlossaryService _glossaryService;

    public GlossaryController(IGlossaryService glossaryService)
    {
        _glossaryService = glossaryService;
    }

    [HttpPost("terms")]
    public ActionResult<ApiResponse<GlossaryTerm>> CreateTerm([FromBody] GlossaryTermRequest request)
    {
        var term = _glossaryService.CreateTerm(request);
        return Ok(ApiResponse.From(term, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet("terms")]
    public ActionResult<ApiResponse<IEnumerable<GlossaryTerm>>> GetTerms()
    {
        var terms = _glossaryService.GetTerms();
        return Ok(ApiResponse.From(terms, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
