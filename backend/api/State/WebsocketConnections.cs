using System.Collections.Concurrent;
using api.Models;
using api.Models.Enums;

namespace api.State;

public static class WebsocketConnections
{
    public static readonly ConcurrentDictionary<Guid, WebsocketMetadata> ConnectionPool = new();
    public static readonly ConcurrentDictionary<TopicEnums, ConcurrentBag<Guid>> TopicSubscriptions = new();
}