using AIHub.Api.Models;
using AIHub.Api.Services;

namespace AIHub.Api.Application.Glossary;

public interface IGlossaryService
{
    GlossaryTerm CreateTerm(GlossaryTermRequest request);

    IEnumerable<GlossaryTerm> GetTerms();
}

public sealed class GlossaryService : IGlossaryService
{
    private readonly InMemoryStore _store;

    public GlossaryService(InMemoryStore store)
    {
        _store = store;
    }

    public GlossaryTerm CreateTerm(GlossaryTermRequest request)
    {
        var term = new GlossaryTerm(
            Id: Guid.NewGuid(),
            Term: request.Term.Trim(),
            Definition: request.Definition.Trim(),
            CreatedAt: DateTimeOffset.UtcNow);

        _store.GlossaryTerms[term.Id] = term;

        return term;
    }

    public IEnumerable<GlossaryTerm> GetTerms()
    {
        return _store.GlossaryTerms.Values.OrderBy(term => term.Term);
    }
}
