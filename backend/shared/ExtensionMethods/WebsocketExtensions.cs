using System.Collections.Concurrent;
using core.Models;
using Fleck;

namespace core.ExtensionMethods;

public static class WebsocketExtensions
{
    public static ConcurrentDictionary<Guid, WebSocketInfo> LiveConnections = new();

    public static void Authenticate(this IWebSocketConnection connection, EndUser userInfo)
    {
        LiveConnections[connection.ConnectionInfo.Id].isAuthenticated = true;
        LiveConnections[connection.ConnectionInfo.Id].userInfo = userInfo;
    }

    public static void UnAuthenticate(this IWebSocketConnection connection)
    {
        LiveConnections[connection.ConnectionInfo.Id].isAuthenticated = false;
    }

    public static void JoinRoom(this IWebSocketConnection connection, int roomId)
    {
        LiveConnections[connection.ConnectionInfo.Id].connectedRooms.Add(roomId);
    }

    public static void RemoveFromRoom(this IWebSocketConnection connection, int roomId)
    {
        LiveConnections[connection.ConnectionInfo.Id].connectedRooms.Remove(roomId);
    }

    public static WebSocketInfo GetMetadata(this IWebSocketConnection connection)
    {
        return LiveConnections[connection.ConnectionInfo.Id];
    }
}

public class WebSocketInfo
{
    public IWebSocketConnection? socket { get; set; }

    public bool isAuthenticated { get; set; }

    public EndUser userInfo { get; set; }
    public HashSet<int> connectedRooms { get; set; }
}