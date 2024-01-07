using System.Collections.Concurrent;
using System.Security.Authentication;
using System.Text.Json;
using api.Models;
using api.Models.DbModels;
using api.Models.Enums;
using api.Models.ServerEvents;
using api.State;
using Fleck;

namespace api.Extensions;

public static class WebSocketExtensions
{
    //3

    public static void AddConnection(this IWebSocketConnection ws)
    {
        var conn = new WebsocketMetadata
        {
            Socket = ws
        };
        WebsocketConnections.ConnectionPool.TryAdd(ws.ConnectionInfo.Id, conn);
    }

    public static void RemoveFromConnections(this IWebSocketConnection ws)
    {
        WebsocketConnections.ConnectionPool.TryRemove(ws.ConnectionInfo.Id, out var _);
    }

    public static void SubscribeToTopic(this IWebSocketConnection ws, TopicEnums topic)
    {
        var bag = WebsocketConnections.TopicSubscriptions.GetOrAdd(topic, _ => new ConcurrentBag<Guid>());
        bag.Add(ws.ConnectionInfo.Id);
    }


    public static void SendBaseDto(this IWebSocketConnection websocket, BaseDto dto)
    {
        var dtoString = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        websocket.Send(dtoString);
    }

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

    public static WebsocketMetadata GetMetadata(this IWebSocketConnection connection)
    {
        return WebsocketConnections.ConnectionPool[connection.ConnectionInfo.Id];
    }

    public static void SendDto<T>(this IWebSocketConnection socket, T dto) where T : BaseDto
    {
        var serialized = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        socket.Send(serialized);
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
        });
    }

    public static void RemoveFromWebsocketConnections(this IWebSocketConnection connection)
    {
        WebsocketConnections.ConnectionPool.TryRemove(connection.ConnectionInfo.Id, out var metadata);
        foreach (var topic in WebsocketConnections.TopicSubscriptions.Keys)
            connection.UnsubscribeFromTopic(topic);
    }

    public static void ExitIfNotAuthenticated(this IWebSocketConnection socket)
    {
        if (!WebsocketConnections.ConnectionPool.ContainsKey(socket.ConnectionInfo.Id) 
            || !socket.GetMetadata().IsAuthenticated)
            throw new AuthenticationException("Unauthorized access.");
    }

    public static void UnsubscribeFromTopic(this IWebSocketConnection socket, TopicEnums topic)
    {
        if (WebsocketConnections.TopicSubscriptions.ContainsKey(topic))
        {
            var bag = WebsocketConnections.TopicSubscriptions[topic];
            var newBag = new ConcurrentBag<Guid>(bag.Where(id => id != socket.ConnectionInfo.Id));
            WebsocketConnections.TopicSubscriptions[topic] = newBag;
        }
    }
}