using AIHub.Api.Application.Policies;
using AIHub.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("policies")]
public sealed class PolicyController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PolicyController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    [HttpGet]
    public ActionResult<ApiResponse<IEnumerable<PolicyDefinition>>> GetPolicies()
    {
        var policies = _policyService.GetPolicies();
        return Ok(ApiResponse.From(policies, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost]
    public ActionResult<ApiResponse<PolicyDefinition>> Create([FromBody] PolicyRequest request)
    {
        var policy = _policyService.Create(request);
        return Ok(ApiResponse.From(policy, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
