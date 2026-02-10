using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AIHub.Api.Services;

public interface IMinimaxChatService
{
    Task<string?> TrySendAsync(string userMessage, CancellationToken cancellationToken = default);
}

public sealed class MinimaxChatService : IMinimaxChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MinimaxChatService> _logger;

    public MinimaxChatService(HttpClient httpClient, ILogger<MinimaxChatService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> TrySendAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        var apiKey = Environment.GetEnvironmentVariable("MINIMAX_API_KEY");
        var groupId = Environment.GetEnvironmentVariable("MINIMAX_GROUP_ID");
        var model = Environment.GetEnvironmentVariable("MINIMAX_MODEL") ?? "abab6.5s-chat";
        var baseUrl = Environment.GetEnvironmentVariable("MINIMAX_BASE_URL") ?? "https://api.minimax.chat";

        if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(groupId))
        {
            _logger.LogWarning("MINIMAX_API_KEY hoặc MINIMAX_GROUP_ID chưa được cấu hình.");
            return null;
        }

        var endpoint = $"{baseUrl.TrimEnd('/')}/v1/text/chatcompletion_v2?GroupId={Uri.EscapeDataString(groupId)}";
        var payload = new
        {
            model,
            temperature = 0.2,
            tokens_to_generate = 1024,
            messages = new[]
            {
                new { sender_type = "USER", text = userMessage }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Minimax API lỗi {StatusCode}: {Response}", response.StatusCode, content);
            return null;
        }

        using var document = JsonDocument.Parse(content);
        var text = TryGetResponseText(document.RootElement);
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("Không đọc được nội dung phản hồi từ Minimax. Payload: {Payload}", content);
            return null;
        }

        return text.Trim();
    }

    private static string? TryGetResponseText(JsonElement root)
    {
        if (root.TryGetProperty("reply", out var replyElement) && replyElement.ValueKind == JsonValueKind.String)
        {
            return replyElement.GetString();
        }

        if (root.TryGetProperty("choices", out var choicesElement) &&
            choicesElement.ValueKind == JsonValueKind.Array &&
            choicesElement.GetArrayLength() > 0)
        {
            var firstChoice = choicesElement[0];
            if (firstChoice.TryGetProperty("message", out var messageElement))
            {
                if (messageElement.TryGetProperty("content", out var contentElement) && contentElement.ValueKind == JsonValueKind.String)
                {
                    return contentElement.GetString();
                }

                if (messageElement.TryGetProperty("text", out var textElement) && textElement.ValueKind == JsonValueKind.String)
                {
                    return textElement.GetString();
                }
            }

            if (firstChoice.TryGetProperty("text", out var choiceTextElement) && choiceTextElement.ValueKind == JsonValueKind.String)
            {
                return choiceTextElement.GetString();
            }
        }

        if (root.TryGetProperty("base_resp", out var baseResp) &&
            baseResp.TryGetProperty("status_code", out var statusCodeElement) &&
            statusCodeElement.ValueKind == JsonValueKind.Number &&
            statusCodeElement.GetInt32() != 0)
        {
            return null;
        }

        if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
        {
            return message.GetString();
        }

        return null;
    }
}
