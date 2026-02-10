using AIHub.Api.Application.ApiCatalog;
using AIHub.Api.Application.Approval;
using AIHub.Api.Application.DataGeneration;
using AIHub.Api.Application.Glossary;
using AIHub.Api.Application.Knowledge;
using AIHub.Api.Application.ModelProfiles;
using AIHub.Api.Application.Policies;
using AIHub.Api.Hubs;
using AIHub.Api.Services;
using AIHub.Modules.ChatBox;
using AIHub.Modules.Management;
using AIHub.Modules.MockApi;
using AIHub.Modules.Tooling;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<InMemoryStore>();
builder.Services.AddSingleton<ChatRealtimeStore>();

builder.Services.AddScoped<IKnowledgeService, KnowledgeService>();
builder.Services.AddScoped<IGlossaryService, GlossaryService>();
builder.Services.AddScoped<IApiCatalogService, ApiCatalogService>();
builder.Services.AddScoped<IDataGenerationService, DataGenerationService>();
builder.Services.AddScoped<IApprovalService, ApprovalService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddScoped<IModelProfileService, ModelProfileService>();

// Modular architecture
builder.Services.AddSingleton<IManagementService, ManagementService>();
builder.Services.AddSingleton<IMockApiService, MockApiService>();
builder.Services.AddSingleton<IToolGateway, ToolGateway>();
builder.Services.AddSingleton<IChatBoxService, ChatBoxService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
