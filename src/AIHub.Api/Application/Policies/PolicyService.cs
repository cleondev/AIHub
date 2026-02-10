using AIHub.Api.Models;
using AIHub.Api.Services;

namespace AIHub.Api.Application.Policies;

public interface IPolicyService
{
    IEnumerable<PolicyDefinition> GetPolicies();

    PolicyDefinition Create(PolicyRequest request);
}

public sealed class PolicyService : IPolicyService
{
    private readonly InMemoryStore _store;

    public PolicyService(InMemoryStore store)
    {
        _store = store;
    }

    public IEnumerable<PolicyDefinition> GetPolicies()
    {
        return _store.Policies.Values.OrderBy(policy => policy.Name);
    }

    public PolicyDefinition Create(PolicyRequest request)
    {
        var policy = new PolicyDefinition(
            Id: Guid.NewGuid(),
            Name: request.Name.Trim(),
            Description: request.Description.Trim(),
            Rules: request.Rules,
            CreatedAt: DateTimeOffset.UtcNow);

        _store.Policies[policy.Id] = policy;

        return policy;
    }
}
