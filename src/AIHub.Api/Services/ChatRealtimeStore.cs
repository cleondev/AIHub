using System.Collections.Concurrent;

namespace AIHub.Api.Services;

public sealed record ChatRealtimeMessage(string Sender, string Message, DateTimeOffset SentAt);

public sealed class ChatRealtimeStore
{
    private readonly ConcurrentQueue<ChatRealtimeMessage> _messages = new();
    private const int MaxMessages = 100;

    public IReadOnlyCollection<ChatRealtimeMessage> GetRecent()
    {
        return _messages.ToArray();
    }

    public ChatRealtimeMessage Append(string sender, string message)
    {
        var item = new ChatRealtimeMessage(sender, message, DateTimeOffset.UtcNow);
        _messages.Enqueue(item);

        while (_messages.Count > MaxMessages)
        {
            _messages.TryDequeue(out _);
        }

        return item;
    }
}
