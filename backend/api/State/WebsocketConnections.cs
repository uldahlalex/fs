using System.Collections.Concurrent;
using Fleck;
using Infrastructure.DbModels;

namespace api.State;

public static class WebsocketConnections
{
    public static ConcurrentDictionary<Guid, WebsocketMetadata> ConnectionPool = new();
    public static ConcurrentDictionary<string, ConcurrentBag<Guid>> TopicSubscriptions = new();
}

public class WebsocketMetadata
{
    public IWebSocketConnection? Socket { get; set; }
    public bool IsAuthenticated { get; set; }
    public EndUser UserInfo { get; set; } = null!;
}