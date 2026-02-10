using AIHub.Api.Models;
using AIHub.Api.Services;

namespace AIHub.Api.Application.ApiCatalog;

public interface IApiCatalogService
{
    ApiCatalogEntry Create(ApiCatalogRequest request);

    IEnumerable<ApiCatalogEntry> GetAll();
}

public sealed class ApiCatalogService : IApiCatalogService
{
    private readonly InMemoryStore _store;

    public ApiCatalogService(InMemoryStore store)
    {
        _store = store;
    }

    public ApiCatalogEntry Create(ApiCatalogRequest request)
    {
        var entry = new ApiCatalogEntry(
            Id: Guid.NewGuid(),
            Name: request.Name.Trim(),
            Description: request.Description.Trim(),
            Schema: request.Schema,
            CreatedAt: DateTimeOffset.UtcNow);

        _store.ApiCatalog[entry.Id] = entry;

        return entry;
    }

    public IEnumerable<ApiCatalogEntry> GetAll()
    {
        return _store.ApiCatalog.Values.OrderBy(entry => entry.Name);
    }
}
