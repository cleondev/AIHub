using AIHub.Modules.MockApi;

namespace AIHub.Modules.Tooling;

public interface IToolGateway
{
    IEnumerable<MockRequest> ReadRequests(string? keyword);
    MockRequest WriteCreateRequest(string name);
    MockRequest? WriteApproveRequest(Guid id);
}

public sealed class ToolGateway : IToolGateway
{
    private readonly IMockApiService _mockApiService;

    public ToolGateway(IMockApiService mockApiService)
    {
        _mockApiService = mockApiService;
    }

    public IEnumerable<MockRequest> ReadRequests(string? keyword) => _mockApiService.Query(keyword);

    public MockRequest WriteCreateRequest(string name) => _mockApiService.Create(name);

    public MockRequest? WriteApproveRequest(Guid id) => _mockApiService.Approve(id);
}
