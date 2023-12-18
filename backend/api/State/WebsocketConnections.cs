using System.Collections.Concurrent;
using api.Models;

namespace api.State;

public static class WebsocketConnections
{
    public static ConcurrentDictionary<Guid, WebsocketMetadata> ConnectionPool = new();
    public static ConcurrentDictionary<string, ConcurrentBag<Guid>> TopicSubscriptions = new();
}