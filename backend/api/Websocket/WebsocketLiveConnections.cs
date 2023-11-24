using System.Collections.Concurrent;
using Fleck;

namespace api.Websocket;

public class WebsocketLiveConnections
{ 
    public readonly ConcurrentDictionary<Guid, IWebSocketConnection> SocketState = new(); 
}