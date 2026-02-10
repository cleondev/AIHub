using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AIHub.Modules.MockApi;
using AIHub.Modules.SemanticKernel;

namespace AIHub.Api.Services;

public sealed record ExternalChatReply(string Message, IReadOnlyList<ToolCallTrace> ToolCalls);

public interface IMinimaxChatService
{
    Task<ExternalChatReply?> TrySendAsync(string userMessage, CancellationToken cancellationToken = default);
}

public sealed class MinimaxChatService : IMinimaxChatService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MinimaxChatService> _logger;
    private readonly IMockApiService _mockApiService;

    public MinimaxChatService(HttpClient httpClient, ILogger<MinimaxChatService> logger, IMockApiService mockApiService)
    {
        _httpClient = httpClient;
        _logger = logger;
        _mockApiService = mockApiService;
    }

    public async Task<ExternalChatReply?> TrySendAsync(string userMessage, CancellationToken cancellationToken = default)
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
        var toolCalls = new List<ToolCallTrace>();
        var prompt = BuildSystemPrompt(userMessage);

        for (var step = 0; step < 3; step++)
        {
            var modelResponse = await SendToMinimaxAsync(endpoint, apiKey, model, prompt, cancellationToken);
            if (string.IsNullOrWhiteSpace(modelResponse))
            {
                return null;
            }

            var toolRequest = TryParseToolRequest(modelResponse!);
            if (toolRequest is null)
            {
                return new ExternalChatReply(modelResponse.Trim(), toolCalls);
            }

            var toolTrace = ExecuteTool(toolRequest, out var toolResultJson);
            toolCalls.Add(toolTrace);

            if (!toolTrace.Success)
            {
                return new ExternalChatReply(
                    $"Không thể thực thi tool '{toolRequest.Tool}'. Chi tiết: {toolTrace.Error}",
                    toolCalls);
            }

            prompt = $"{prompt}\n\n[TOOL_RESULT]\n{toolResultJson}\n[/TOOL_RESULT]\nHãy tạo câu trả lời tiếng Việt ngắn gọn, không trả về JSON.";
        }

        return new ExternalChatReply("Tôi đã gọi tool nhưng chưa tạo được câu trả lời cuối cùng.", toolCalls);
    }

    private async Task<string?> SendToMinimaxAsync(
        string endpoint,
        string apiKey,
        string model,
        string prompt,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            model,
            temperature = 0.1,
            tokens_to_generate = 1024,
            messages = new[]
            {
                new { sender_type = "USER", text = prompt }
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

        return text;
    }

    private ToolCallTrace ExecuteTool(ToolRequest request, out string toolResultJson)
    {
        try
        {
            if (string.Equals(request.Tool, "list_products", StringComparison.OrdinalIgnoreCase))
            {
                var products = _mockApiService
                    .ListProducts(request.Keyword, request.Name, request.Category)
                    .Select(item => new
                    {
                        item.Id,
                        item.Sku,
                        item.Name,
                        item.Category,
                        item.UnitPrice,
                        item.StockQuantity
                    })
                    .ToList();

                toolResultJson = JsonSerializer.Serialize(new { products });
                return new ToolCallTrace(
                    "MockApi.ListProducts",
                    new { request.Keyword, request.Name, request.Category },
                    products,
                    true);
            }

            if (string.Equals(request.Tool, "create_order", StringComparison.OrdinalIgnoreCase))
            {
                if (!request.ProductId.HasValue)
                {
                    toolResultJson = "{\"error\":\"missing_product_id\"}";
                    return new ToolCallTrace(
                        "MockApi.CreatePurchaseRequest",
                        new { request.ProductId, request.Quantity },
                        null,
                        false,
                        "missing_product_id");
                }

                var order = _mockApiService.CreatePurchaseRequest(request.ProductId.Value, request.Quantity ?? 1);
                toolResultJson = JsonSerializer.Serialize(new
                {
                    order.Id,
                    order.ProductId,
                    order.ProductSku,
                    order.ProductName,
                    order.Quantity,
                    order.Status,
                    order.Message,
                    order.CreatedAt
                });

                return new ToolCallTrace(
                    "MockApi.CreatePurchaseRequest",
                    new { request.ProductId, request.Quantity },
                    order,
                    true);
            }

            toolResultJson = "{\"error\":\"unknown_tool\"}";
            return new ToolCallTrace("Minimax.ToolDispatch", new { request.Tool }, null, false, "unknown_tool");
        }
        catch (Exception ex)
        {
            toolResultJson = JsonSerializer.Serialize(new { error = ex.Message });
            return new ToolCallTrace($"MockApi.{request.Tool}", request, null, false, ex.Message);
        }
    }

    private static string BuildSystemPrompt(string userMessage)
    {
        return $$$"""
            Bạn là trợ lý bán hàng AI. Bạn có thể gọi tool nội bộ bằng cách CHỈ trả về JSON với schema:
            {{"tool":"list_products","keyword":"...","name":"...","category":"..."}}
            hoặc
            {{"tool":"create_order","productId":"guid","quantity":1}}

            Quy tắc:
            - Nếu user hỏi danh sách/sản phẩm/tồn kho, hãy gọi list_products trước.
            - Khi user chỉ định tên/category thì map vào trường name/category.
            - Nếu user muốn tạo request/order mua hàng, gọi create_order.
            - Khi đã nhận [TOOL_RESULT], hãy trả lời tiếng Việt cho user (không JSON nữa).

            Yêu cầu người dùng: {{userMessage}}
            """;
    }


    private static ToolRequest? TryParseToolRequest(string text)
    {
        var trimmed = text.Trim();
        if (!trimmed.StartsWith("{") || !trimmed.EndsWith("}"))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ToolRequest>(trimmed, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
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

    private sealed record ToolRequest(
        string Tool,
        string? Keyword,
        string? Name,
        string? Category,
        Guid? ProductId,
        int? Quantity);
}
