using AIHub.Api.Application.ApiCatalog;
using AIHub.Api.Application.Approval;
using AIHub.Api.Application.DataGeneration;
using AIHub.Api.Application.Glossary;
using AIHub.Api.Application.Knowledge;
using AIHub.Api.Application.ModelProfiles;
using AIHub.Api.Application.Policies;
using AIHub.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton<InMemoryStore>();

builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<IGlossaryService, GlossaryService>();
builder.Services.AddScoped<IApiCatalogService, ApiCatalogService>();
builder.Services.AddScoped<IDataGenerationService, DataGenerationService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IModelProfileService, ModelProfileService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
