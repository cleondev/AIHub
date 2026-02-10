using AIHub.Api.Application.ModelProfiles;
using AIHub.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("models")]
public sealed class ModelProfilesController : ControllerBase
{
    private readonly IModelProfileService _modelProfileService;

    public ModelProfilesController(IModelProfileService modelProfileService)
    {
        _modelProfileService = modelProfileService;
    }

    [HttpGet]
    public ActionResult<ApiResponse<IEnumerable<ModelProfile>>> GetModels()
    {
        var models = _modelProfileService.GetModels();
        return Ok(ApiResponse.From(models, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost]
    public ActionResult<ApiResponse<ModelProfile>> Create([FromBody] ModelProfileRequest request)
    {
        var model = _modelProfileService.Create(request);
        return Ok(ApiResponse.From(model, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
