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

    public static void SubscribeToTopic(this IWebSocketConnection connection, string topic)
    {
        ConnectionPool[connection.ConnectionInfo.Id].subscribedToTopics.Add(topic);
    }

    public static void UnsubscribeFromTopic(this IWebSocketConnection connection, string topic)
    {
        ConnectionPool[connection.ConnectionInfo.Id].subscribedToTopics.Remove(topic);
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
            subscribedToTopics = new HashSet<string>()
        });
    }

    public static void RemoveFromConnectionPool(this IWebSocketConnection connection)
    {
        ConnectionPool.TryRemove(connection.ConnectionInfo.Id, out _);
    }

    public static int CountUsersInRoom(this IWebSocketConnection connection, string topic)
    {
        return ConnectionPool.Values.Count(x => x.subscribedToTopics.Contains(topic));
    }

    public static void BroadcastToTopic(string topic, string message)
    {
        foreach (var socket in ConnectionPool.Values.Where(x => x.subscribedToTopics.Contains(topic)))
            socket.socket!.Send(message);
    }

    public static void BroadCastToAllClients(string message)
    {
        foreach (var keyValuePair in ConnectionPool) keyValuePair.Value.socket!.Send(message);
    }
}

public class WebSocketInfo
{
    public IWebSocketConnection? socket { get; set; }
    public bool isAuthenticated { get; set; }
    public EndUser userInfo { get; set; }
    public HashSet<string> subscribedToTopics { get; set; }
}