using core.Models.DbModels;
using core.State;
using Fleck;

namespace core.ExtensionMethods;

public static class WebsocketExtensions
{

    public static void Authenticate(this IWebSocketConnection connection, EndUser userInfo)
    {
        WebsocketConnections.ConnectionPool[connection.ConnectionInfo.Id].isAuthenticated = true;
        userInfo.hash = null;
        userInfo.salt = null;
        WebsocketConnections.ConnectionPool[connection.ConnectionInfo.Id].userInfo = userInfo;
    }

    public static void UnAuthenticate(this IWebSocketConnection connection)
    {
        WebsocketConnections.ConnectionPool[connection.ConnectionInfo.Id].isAuthenticated = false;
    }

    public static void SubscribeToTopic(this IWebSocketConnection connection, string topic)
    {
        WebsocketConnections.ConnectionPool[connection.ConnectionInfo.Id].subscribedToTopics.Add(topic);
    }

    public static void UnsubscribeFromTopic(this IWebSocketConnection connection, string topic)
    {
        WebsocketConnections.ConnectionPool[connection.ConnectionInfo.Id].subscribedToTopics.Remove(topic);
    }

    public static WebsocketMetadata GetMetadata(this IWebSocketConnection connection)
    {
        return WebsocketConnections.ConnectionPool[connection.ConnectionInfo.Id];
    }
    
    public static int CountUsersInRoom(this IWebSocketConnection connection, string topic)
    {
        return WebsocketConnections.ConnectionPool.Values.Count(x => x.subscribedToTopics.Contains(topic));
    }

    public static void SendDto(this IWebSocketConnection socket, object dto)
    {
        socket.Send(dto.ToJsonString());
    }

    public static bool IsInWebsocketConnections(this IWebSocketConnection connection)
    {
        return WebsocketConnections.ConnectionPool.ContainsKey(connection.ConnectionInfo.Id);
    }
    
    public static void AddToWebsocketConnections(this IWebSocketConnection connection)
    {
        WebsocketConnections.ConnectionPool.TryAdd(connection.ConnectionInfo.Id, new WebsocketMetadata
        {
            socket = connection,
            isAuthenticated = false,
            userInfo = new EndUser(),
            subscribedToTopics = new HashSet<string>()
        });
    }
    
    public static void RemoveFromWebsocketConnections(this IWebSocketConnection connection)
    {
        WebsocketConnections.ConnectionPool.TryRemove(connection.ConnectionInfo.Id, out _);
    }



    public static void BroadcastToAllClients(object dto)
    {
        foreach (var keyValuePair in WebsocketConnections.ConnectionPool) keyValuePair.Value.socket!.Send(dto.ToJsonString());
    }
}

