using System.Collections.Concurrent;
using core.Models;
using Fleck;

namespace core.ExtensionMethods;

public static class WebsocketExtensions
{
    public static ConcurrentDictionary<Guid, WebSocketInfo> ConnectionPool = new();

    public static void Authenticate(this IWebSocketConnection connection, EndUser userInfo)
    {
        ConnectionPool[connection.ConnectionInfo.Id].isAuthenticated = true;
        ConnectionPool[connection.ConnectionInfo.Id].userInfo = userInfo;
    }

    public static void UnAuthenticate(this IWebSocketConnection connection)
    {
        ConnectionPool[connection.ConnectionInfo.Id].isAuthenticated = false;
    }

    public static void JoinRoom(this IWebSocketConnection connection, int roomId)
    {
        ConnectionPool[connection.ConnectionInfo.Id].connectedRooms.Add(roomId);
    }

    public static void RemoveFromRoom(this IWebSocketConnection connection, int roomId)
    {
        ConnectionPool[connection.ConnectionInfo.Id].connectedRooms.Remove(roomId);
    }

    public static WebSocketInfo GetMetadata(this IWebSocketConnection connection)
    {
        return ConnectionPool[connection.ConnectionInfo.Id];
    }

    public static bool IsInConnectionPool(this IWebSocketConnection connection)
    {
        return ConnectionPool.ContainsKey(connection.ConnectionInfo.Id);
    }

    public static void AddToConnectionPool(this IWebSocketConnection connection)
    {
        ConnectionPool.TryAdd(connection.ConnectionInfo.Id, new WebSocketInfo
        {
            socket = connection,
            isAuthenticated = false,
            userInfo = new EndUser(),
            connectedRooms = new HashSet<int>()
        });
    }

    public static void RemoveFromConnectionPool(this IWebSocketConnection connection)
    {
        ConnectionPool.TryRemove(connection.ConnectionInfo.Id, out _);
    }

    public static int CountUsersInRoom(this IWebSocketConnection connection, int roomId)
    {
        return ConnectionPool.Values.Count(x => x.connectedRooms.Contains(roomId));
    }

    public static void BroadcastToRoom(int roomId, string message)
    {
        foreach (var socket in ConnectionPool.Values.Where(x => x.connectedRooms.Contains(roomId)))
            socket.socket.Send(message);
    }
}

public class WebSocketInfo
{
    public IWebSocketConnection? socket { get; set; }

    public bool isAuthenticated { get; set; }

    public EndUser userInfo { get; set; }
    public HashSet<int> connectedRooms { get; set; }
}