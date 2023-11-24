using System.Collections.Concurrent;
using Fleck;

namespace api;

public class WebsocketLiveConnections
{ 
    public readonly ConcurrentDictionary<Guid, IWebSocketConnection> SocketState = new(); 
}