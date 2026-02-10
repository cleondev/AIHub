namespace AIHub.Modules.MockApi;

public enum MockRequestStatus
{
    Pending,
    Approved
}

public sealed record MockRequest(Guid Id, string Name, MockRequestStatus Status, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);

public interface IMockApiService
{
    IEnumerable<MockRequest> Query(string? keyword);
    MockRequest Create(string name);
    MockRequest? Approve(Guid id);
}

public sealed class MockApiService : IMockApiService
{
    private readonly Dictionary<Guid, MockRequest> _requests = [];

    public IEnumerable<MockRequest> Query(string? keyword)
    {
        var normalized = keyword?.Trim();
        var records = _requests.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(normalized))
        {
            records = records.Where(item => item.Name.Contains(normalized, StringComparison.OrdinalIgnoreCase));
        }

        return records.OrderBy(item => item.CreatedAt);
    }

    public MockRequest Create(string name)
    {
        var item = new MockRequest(Guid.NewGuid(), name.Trim(), MockRequestStatus.Pending, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);
        _requests[item.Id] = item;
        return item;
    }

    public MockRequest? Approve(Guid id)
    {
        if (!_requests.TryGetValue(id, out var current))
        {
            return null;
        }

        var approved = current with
        {
            Status = MockRequestStatus.Approved,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _requests[id] = approved;
        return approved;
    }
}
