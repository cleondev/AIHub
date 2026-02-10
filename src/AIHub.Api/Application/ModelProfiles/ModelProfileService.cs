using AIHub.Api.Models;
using AIHub.Api.Services;

namespace AIHub.Api.Application.ModelProfiles;

public interface IModelProfileService
{
    IEnumerable<ModelProfile> GetModels();

    ModelProfile Create(ModelProfileRequest request);
}

public sealed class ModelProfileService : IModelProfileService
{
    private readonly InMemoryStore _store;

    public ModelProfileService(InMemoryStore store)
    {
        _store = store;
    }

    public IEnumerable<ModelProfile> GetModels()
    {
        return _store.ModelProfiles.Values.OrderBy(model => model.Name);
    }

    public ModelProfile Create(ModelProfileRequest request)
    {
        var model = new ModelProfile(
            Id: Guid.NewGuid(),
            Name: request.Name.Trim(),
            Provider: request.Provider.Trim(),
            Model: request.Model.Trim(),
            Temperature: request.Temperature,
            CreatedAt: DateTimeOffset.UtcNow);

        _store.ModelProfiles[model.Id] = model;

        return model;
    }
}
