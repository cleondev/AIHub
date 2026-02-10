using AIHub.Api.Application.ApiCatalog;
using AIHub.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("api-catalog")]
public sealed class ApiCatalogController : ControllerBase
{
    private readonly IApiCatalogService _apiCatalogService;

    public ApiCatalogController(IApiCatalogService apiCatalogService)
    {
        _apiCatalogService = apiCatalogService;
    }

    [HttpPost]
    public ActionResult<ApiResponse<ApiCatalogEntry>> Create([FromBody] ApiCatalogRequest request)
    {
        var entry = _apiCatalogService.Create(request);
        return Ok(ApiResponse.From(entry, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet]
    public ActionResult<ApiResponse<IEnumerable<ApiCatalogEntry>>> GetAll()
    {
        var entries = _apiCatalogService.GetAll();
        return Ok(ApiResponse.From(entries, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
