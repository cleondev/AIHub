using AIHub.Api.Models;
using AIHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("api-catalog")]
public sealed class ApiCatalogController : ControllerBase
{
    private readonly InMemoryStore _store;

    public ApiCatalogController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpPost]
    public ActionResult<ApiResponse<ApiCatalogEntry>> Create([FromBody] ApiCatalogRequest request)
    {
        var entry = new ApiCatalogEntry(
            Id: Guid.NewGuid(),
            Name: request.Name.Trim(),
            Description: request.Description.Trim(),
            Schema: request.Schema,
            CreatedAt: DateTimeOffset.UtcNow);

        _store.ApiCatalog[entry.Id] = entry;

        return Ok(ApiResponse.From(entry, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet]
    public ActionResult<ApiResponse<IEnumerable<ApiCatalogEntry>>> GetAll()
    {
        var entries = _store.ApiCatalog.Values.OrderBy(entry => entry.Name);
        return Ok(ApiResponse.From(entries, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
