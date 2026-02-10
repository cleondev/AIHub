using AIHub.Api.Models;
using AIHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("models")]
public sealed class ModelProfilesController : ControllerBase
{
    private readonly InMemoryStore _store;

    public ModelProfilesController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<ApiResponse<IEnumerable<ModelProfile>>> GetModels()
    {
        var models = _store.ModelProfiles.Values.OrderBy(model => model.Name);
        return Ok(ApiResponse.From(models, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost]
    public ActionResult<ApiResponse<ModelProfile>> Create([FromBody] ModelProfileRequest request)
    {
        var model = new ModelProfile(
            Id: Guid.NewGuid(),
            Name: request.Name.Trim(),
            Provider: request.Provider.Trim(),
            Model: request.Model.Trim(),
            Temperature: request.Temperature,
            CreatedAt: DateTimeOffset.UtcNow);

        _store.ModelProfiles[model.Id] = model;

        return Ok(ApiResponse.From(model, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
