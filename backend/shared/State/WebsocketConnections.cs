using System.Collections.Concurrent;
using core.Models.DbModels;
using Fleck;

namespace core.State;

public static class WebsocketConnections
{
    public static ConcurrentDictionary<Guid, WebsocketMetadata> ConnectionPool = new();

}
public class WebsocketMetadata
{
    public IWebSocketConnection? socket { get; set; }
    public bool isAuthenticated { get; set; }
    public EndUser userInfo { get; set; } = null!;
    public HashSet<string> subscribedToTopics { get; set; } = new HashSet<string>();
}