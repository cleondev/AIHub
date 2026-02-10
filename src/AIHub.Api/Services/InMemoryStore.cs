using System.Collections.Concurrent;
using AIHub.Api.Models;

namespace AIHub.Api.Services;

public sealed class InMemoryStore
{
    public ConcurrentDictionary<Guid, Document> Documents { get; } = new();

    public ConcurrentDictionary<Guid, GlossaryTerm> GlossaryTerms { get; } = new();

    public ConcurrentDictionary<Guid, ApiCatalogEntry> ApiCatalog { get; } = new();

    public ConcurrentDictionary<Guid, DataGenerationDraft> DataDrafts { get; } = new();

    public ConcurrentDictionary<Guid, PolicyDefinition> Policies { get; } = new();

    public ConcurrentDictionary<Guid, ModelProfile> ModelProfiles { get; } = new();
}
