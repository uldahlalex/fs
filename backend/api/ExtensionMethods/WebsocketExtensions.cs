using api.Helpers;
using api.Models;
using api.Models.Enums;
using api.State;
using Fleck;
using Infrastructure.DbModels;

namespace api.ExtensionMethods;

public static class WebsocketExtensions
{
    public static void Authenticate(this IWebSocketConnection connection, EndUser userInfo)
    {
        var metadata = connection.GetMetadata();
        metadata.IsAuthenticated = true;
        userInfo.hash = null;
        userInfo.salt = null;
        metadata.UserInfo = userInfo;
    }

    public static void UnAuthenticate(this IWebSocketConnection connection)
    {
        var metadata = connection.GetMetadata();
        metadata.IsAuthenticated = false;
    }

    public static void SubscribeToTopic(this IWebSocketConnection connection, TopicEnums topic)
    {
        WebsocketHelpers.SubscribeToTopic(connection.ConnectionInfo.Id, topic);
    }

    public static void UnsubscribeFromTopic(this IWebSocketConnection connection, TopicEnums topic)
    {
        WebsocketHelpers.UnsubscribeFromTopic(connection.ConnectionInfo.Id, topic);
    }

    public static WebsocketMetadata GetMetadata(this IWebSocketConnection connection)
    {
        return WebsocketConnections.ConnectionPool[connection.ConnectionInfo.Id];
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
            Socket = connection,
            IsAuthenticated = false,
            UserInfo = new EndUser()
            // Note: Subscribed topics are now managed separately
        });
    }

    public static void RemoveFromWebsocketConnections(this IWebSocketConnection connection)
    {
        if (WebsocketConnections.ConnectionPool.TryRemove(connection.ConnectionInfo.Id, out var metadata))
            // Clean up any topic subscriptions
            foreach (var topic in WebsocketConnections.TopicSubscriptions.Keys)
                WebsocketHelpers.UnsubscribeFromTopic(connection.ConnectionInfo.Id, topic);
    }
}