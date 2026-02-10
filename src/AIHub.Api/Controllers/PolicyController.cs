using AIHub.Api.Models;
using AIHub.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIHub.Api.Controllers;

[ApiController]
[Route("policies")]
public sealed class PolicyController : ControllerBase
{
    private readonly InMemoryStore _store;

    public PolicyController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<ApiResponse<IEnumerable<PolicyDefinition>>> GetPolicies()
    {
        var policies = _store.Policies.Values.OrderBy(policy => policy.Name);
        return Ok(ApiResponse.From(policies, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }

    [HttpPost]
    public ActionResult<ApiResponse<PolicyDefinition>> Create([FromBody] PolicyRequest request)
    {
        var policy = new PolicyDefinition(
            Id: Guid.NewGuid(),
            Name: request.Name.Trim(),
            Description: request.Description.Trim(),
            Rules: request.Rules,
            CreatedAt: DateTimeOffset.UtcNow);

        _store.Policies[policy.Id] = policy;

        return Ok(ApiResponse.From(policy, TraceIdProvider.GetFromHttpContext(HttpContext)));
    }
}
