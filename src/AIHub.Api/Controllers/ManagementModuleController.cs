using AIHub.Api.Models;
using AIHub.Modules.Management;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("module/management")]
public sealed class ManagementModuleController : ControllerBase
{
    private readonly IManagementService _managementService;

    public ManagementModuleController(IManagementService managementService)
    {
        _managementService = managementService;
    }

    [HttpPost("concepts")]
    public ActionResult<ApiResponse<KnowledgeConcept>> AddConcept([FromBody] AddConceptRequest request)
    {
        var concept = _managementService.AddConcept(request.Name);
        return Ok(ApiResponse.From(concept, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet("concepts")]
    public ActionResult<ApiResponse<IEnumerable<KnowledgeConcept>>> GetConcepts()
    {
        return Ok(ApiResponse.From(_managementService.GetConcepts(), TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet("llm-config")]
    public ActionResult<ApiResponse<LlmProviderConfig>> GetLlmConfig()
    {
        return Ok(ApiResponse.From(_managementService.GetLlmConfig(), TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("llm-config")]
    public ActionResult<ApiResponse<LlmProviderConfig>> SetLlmConfig([FromBody] SetLlmConfigRequest request)
    {
        var config = _managementService.SetLlmConfig(new LlmProviderConfig(request.Provider, request.Model, request.ApiBaseUrl, request.ApiKey, request.GroupId));
        return Ok(ApiResponse.From(config, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost("tools")]
    public ActionResult<ApiResponse<ToolDefinition>> AddTool([FromBody] AddToolRequest request)
    {
        var tool = _managementService.AddTool(request.Name, request.Description);
        return Ok(ApiResponse.From(tool, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpGet("tools")]
    public ActionResult<ApiResponse<IEnumerable<ToolDefinition>>> GetTools()
    {
        return Ok(ApiResponse.From(_managementService.GetTools(), TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
