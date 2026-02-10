using System.Diagnostics;

namespace AIHub.Api.Models;

public static class ApiResponse
{
    public static ApiResponse<T> From<T>(T data, string traceId) => new(traceId, data);
}

public sealed record ApiResponse<T>(string TraceId, T Data);

public sealed record ErrorResponse(string Code, string Message);

public static class TraceIdProvider
{
    public static string GetFromHttpContext(HttpContext context)
    {
        if (!string.IsNullOrWhiteSpace(context.TraceIdentifier))
        {
            return context.TraceIdentifier;
        }

        return Activity.Current?.Id ?? Guid.NewGuid().ToString();
    }
}
