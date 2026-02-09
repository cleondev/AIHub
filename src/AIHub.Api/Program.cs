using System.Collections.Concurrent;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

var documents = new ConcurrentDictionary<Guid, Document>();
var glossaryTerms = new ConcurrentDictionary<Guid, GlossaryTerm>();
var apiCatalog = new ConcurrentDictionary<Guid, ApiCatalogEntry>();
var dataDrafts = new ConcurrentDictionary<Guid, DataGenerationDraft>();
var policies = new ConcurrentDictionary<Guid, PolicyDefinition>();
var modelProfiles = new ConcurrentDictionary<Guid, ModelProfile>();

app.MapPost("/knowledge/documents", (DocumentUploadRequest request) =>
{
    var document = new Document(
        Id: Guid.NewGuid(),
        Title: request.Title.Trim(),
        SourceType: request.SourceType.Trim(),
        Status: "ingested",
        CreatedAt: DateTimeOffset.UtcNow);

    documents[document.Id] = document;
    return Results.Ok(ApiResponse.From(document));
});

app.MapGet("/knowledge/documents", () =>
    Results.Ok(ApiResponse.From(documents.Values.OrderByDescending(item => item.CreatedAt))));

app.MapPost("/knowledge/query", (KnowledgeQueryRequest request) =>
{
    var response = new KnowledgeQueryResponse(
        Query: request.Query,
        Answer: "Câu trả lời mẫu từ hệ thống AIHub (MVP).",
        Sources:
        [
            new KnowledgeSource("kb:sample-doc", "Sample Document", 0.82m),
            new KnowledgeSource("kb:glossary", "Glossary", 0.76m)
        ]);

    return Results.Ok(ApiResponse.From(response));
});

app.MapPost("/glossary/terms", (GlossaryTermRequest request) =>
{
    var term = new GlossaryTerm(
        Id: Guid.NewGuid(),
        Term: request.Term.Trim(),
        Definition: request.Definition.Trim(),
        CreatedAt: DateTimeOffset.UtcNow);

    glossaryTerms[term.Id] = term;
    return Results.Ok(ApiResponse.From(term));
});

app.MapGet("/glossary/terms", () =>
    Results.Ok(ApiResponse.From(glossaryTerms.Values.OrderBy(term => term.Term))));

app.MapPost("/api-catalog", (ApiCatalogRequest request) =>
{
    var entry = new ApiCatalogEntry(
        Id: Guid.NewGuid(),
        Name: request.Name.Trim(),
        Description: request.Description.Trim(),
        Schema: request.Schema,
        CreatedAt: DateTimeOffset.UtcNow);

    apiCatalog[entry.Id] = entry;
    return Results.Ok(ApiResponse.From(entry));
});

app.MapGet("/api-catalog", () =>
    Results.Ok(ApiResponse.From(apiCatalog.Values.OrderBy(entry => entry.Name))));

app.MapPost("/ai/data-generation", (AiDataGenerationRequest request) =>
{
    var draft = new DataGenerationDraft(
        Id: Guid.NewGuid(),
        Prompt: request.Prompt.Trim(),
        SchemaRef: request.SchemaRef,
        Status: DraftStatus.PendingApproval,
        Payload: request.SeedPayload,
        CreatedAt: DateTimeOffset.UtcNow,
        UpdatedAt: DateTimeOffset.UtcNow);

    dataDrafts[draft.Id] = draft;
    return Results.Ok(ApiResponse.From(draft));
});

app.MapGet("/ai/data-generation/{id:guid}", (Guid id) =>
{
    if (!dataDrafts.TryGetValue(id, out var draft))
    {
        return Results.NotFound(ApiResponse.From(new ErrorResponse("draft_not_found", "Không tìm thấy bản nháp.")));
    }

    return Results.Ok(ApiResponse.From(draft));
});

app.MapGet("/policies", () =>
    Results.Ok(ApiResponse.From(policies.Values.OrderBy(policy => policy.Name))));

app.MapPost("/policies", (PolicyRequest request) =>
{
    var policy = new PolicyDefinition(
        Id: Guid.NewGuid(),
        Name: request.Name.Trim(),
        Description: request.Description.Trim(),
        Rules: request.Rules,
        CreatedAt: DateTimeOffset.UtcNow);

    policies[policy.Id] = policy;
    return Results.Ok(ApiResponse.From(policy));
});

app.MapGet("/models", () =>
    Results.Ok(ApiResponse.From(modelProfiles.Values.OrderBy(model => model.Name))));

app.MapPost("/models", (ModelProfileRequest request) =>
{
    var model = new ModelProfile(
        Id: Guid.NewGuid(),
        Name: request.Name.Trim(),
        Provider: request.Provider.Trim(),
        Model: request.Model.Trim(),
        Temperature: request.Temperature,
        CreatedAt: DateTimeOffset.UtcNow);

    modelProfiles[model.Id] = model;
    return Results.Ok(ApiResponse.From(model));
});

app.MapPost("/approval/{id:guid}/approve", (Guid id, ApprovalRequest request) =>
{
    if (!dataDrafts.TryGetValue(id, out var draft))
    {
        return Results.NotFound(ApiResponse.From(new ErrorResponse("draft_not_found", "Không tìm thấy bản nháp.")));
    }

    var approved = draft with
    {
        Status = DraftStatus.Approved,
        ApprovedBy = request.Actor.Trim(),
        ApprovalComment = request.Comment?.Trim(),
        UpdatedAt = DateTimeOffset.UtcNow
    };

    dataDrafts[id] = approved;
    return Results.Ok(ApiResponse.From(approved));
});

app.MapPost("/approval/{id:guid}/reject", (Guid id, ApprovalRequest request) =>
{
    if (!dataDrafts.TryGetValue(id, out var draft))
    {
        return Results.NotFound(ApiResponse.From(new ErrorResponse("draft_not_found", "Không tìm thấy bản nháp.")));
    }

    var rejected = draft with
    {
        Status = DraftStatus.Rejected,
        ApprovedBy = request.Actor.Trim(),
        ApprovalComment = request.Comment?.Trim(),
        UpdatedAt = DateTimeOffset.UtcNow
    };

    dataDrafts[id] = rejected;
    return Results.Ok(ApiResponse.From(rejected));
});

app.MapGet("/health", () => Results.Ok(ApiResponse.From(new { status = "ok" })));

app.Run();

static class ApiResponse
{
    public static ApiResponse<T> From<T>(T data) => new(TraceIdProvider.Get(), data);
}

static class TraceIdProvider
{
    public static string Get() => Activity.Current?.Id ?? Guid.NewGuid().ToString();
}

record ApiResponse<T>(string TraceId, T Data);

record ErrorResponse(string Code, string Message);

record DocumentUploadRequest(string Title, string SourceType, string? Content);

record Document(Guid Id, string Title, string SourceType, string Status, DateTimeOffset CreatedAt);

record KnowledgeQueryRequest(string Query, int TopK = 5);

record KnowledgeQueryResponse(string Query, string Answer, IReadOnlyList<KnowledgeSource> Sources);

record KnowledgeSource(string Id, string Title, decimal Score);

record GlossaryTermRequest(string Term, string Definition);

record GlossaryTerm(Guid Id, string Term, string Definition, DateTimeOffset CreatedAt);

record ApiCatalogRequest(string Name, string Description, string Schema);

record ApiCatalogEntry(Guid Id, string Name, string Description, string Schema, DateTimeOffset CreatedAt);

record AiDataGenerationRequest(string Prompt, string SchemaRef, string? SeedPayload);

record DataGenerationDraft(
    Guid Id,
    string Prompt,
    string SchemaRef,
    DraftStatus Status,
    string? Payload,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? ApprovedBy = null,
    string? ApprovalComment = null);

enum DraftStatus
{
    PendingApproval,
    Approved,
    Rejected
}

record PolicyRequest(string Name, string Description, IReadOnlyList<string> Rules);

record PolicyDefinition(Guid Id, string Name, string Description, IReadOnlyList<string> Rules, DateTimeOffset CreatedAt);

record ModelProfileRequest(string Name, string Provider, string Model, decimal Temperature = 0.2m);

record ModelProfile(Guid Id, string Name, string Provider, string Model, decimal Temperature, DateTimeOffset CreatedAt);

record ApprovalRequest(string Actor, string? Comment);
