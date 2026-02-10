namespace AIHub.Modules.Management;

public sealed record KnowledgeConcept(Guid Id, string Name, DateTimeOffset CreatedAt);

public sealed record LlmProviderConfig(string Provider, string Model, string ApiBaseUrl, string? ApiKey = null, string? GroupId = null);

public sealed record ToolDefinition(Guid Id, string Name, string Description, DateTimeOffset CreatedAt);

public interface IManagementService
{
    IEnumerable<KnowledgeConcept> GetConcepts();
    KnowledgeConcept AddConcept(string name);
    LlmProviderConfig GetLlmConfig();
    LlmProviderConfig SetLlmConfig(LlmProviderConfig config);
    IEnumerable<ToolDefinition> GetTools();
    ToolDefinition AddTool(string name, string description);
}

public sealed class ManagementService : IManagementService
{
    private readonly List<KnowledgeConcept> _concepts = [];
    private readonly List<ToolDefinition> _tools = [];
    private LlmProviderConfig _llmConfig = new("minimax", "MiniMax-Text-01", "https://api.minimax.chat");

    public IEnumerable<KnowledgeConcept> GetConcepts() => _concepts.OrderBy(item => item.CreatedAt);

    public KnowledgeConcept AddConcept(string name)
    {
        var concept = new KnowledgeConcept(Guid.NewGuid(), name.Trim(), DateTimeOffset.UtcNow);
        _concepts.Add(concept);
        return concept;
    }

    public LlmProviderConfig GetLlmConfig() => _llmConfig;

    public LlmProviderConfig SetLlmConfig(LlmProviderConfig config)
    {
        _llmConfig = config;

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            Environment.SetEnvironmentVariable("MINIMAX_API_KEY", config.ApiKey);
        }

        if (!string.IsNullOrWhiteSpace(config.GroupId))
        {
            Environment.SetEnvironmentVariable("MINIMAX_GROUP_ID", config.GroupId);
        }

        if (!string.IsNullOrWhiteSpace(config.Model))
        {
            Environment.SetEnvironmentVariable("MINIMAX_MODEL", config.Model);
        }

        if (!string.IsNullOrWhiteSpace(config.ApiBaseUrl))
        {
            Environment.SetEnvironmentVariable("MINIMAX_BASE_URL", config.ApiBaseUrl);
        }

        return _llmConfig;
    }

    public IEnumerable<ToolDefinition> GetTools() => _tools.OrderBy(item => item.CreatedAt);

    public ToolDefinition AddTool(string name, string description)
    {
        var tool = new ToolDefinition(Guid.NewGuid(), name.Trim(), description.Trim(), DateTimeOffset.UtcNow);
        _tools.Add(tool);
        return tool;
    }
}
