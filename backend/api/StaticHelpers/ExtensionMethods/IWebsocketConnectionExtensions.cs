using System.Collections.Concurrent;
using System.Text.Json;
using api.Models;
using api.Models.Enums;
using api.State;
using Commons;
using Externalities.QueryModels;
using Fleck;
using lib;
using Serilog;

namespace api.StaticHelpers.ExtensionMethods;

public static class WebSocketExtensions
{
    public static void AddConnection(this IWebSocketConnection ws)
    {
        WebsocketConnections.ConnectionPool.TryAdd(ws.ConnectionInfo.Id,
            new WebsocketMetadata
            {
                Socket = ws
            });
    }


    public static void SubscribeToTopic(this IWebSocketConnection ws, TopicEnums topic)
    {
        WebsocketConnections.TopicSubscriptions.GetOrAdd(topic, _ => new ConcurrentBag<Guid>())
            .Add(ws.ConnectionInfo.Id);
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
        connection.GetMetadata().IsAuthenticated = false;
    }


    public static WebsocketMetadata GetMetadata(this IWebSocketConnection connection)
    {
        return WebsocketConnections.ConnectionPool[connection.ConnectionInfo.Id];
    }


    public static void SendDto<T>(this IWebSocketConnection socket, T dto) where T : BaseDto
    {
        if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == EnvironmentEnums.Testing.ToString())
            Log.Information(JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true }));
        socket.Send(JsonSerializer.Serialize(dto, new JsonSerializerOptions
            { PropertyNameCaseInsensitive = true }));
    }


    public static void RemoveFromWebsocketConnections(this IWebSocketConnection connection)
    {
        WebsocketConnections.ConnectionPool.TryRemove(connection.ConnectionInfo.Id, out _);
        foreach (var topic in WebsocketConnections.TopicSubscriptions.Keys)
            connection.UnsubscribeFromTopic(topic);
    }

    public static bool IsAuthenticated(this IWebSocketConnection socket)
    {
        return WebsocketConnections.ConnectionPool.ContainsKey(socket.ConnectionInfo.Id)
               && socket.GetMetadata().IsAuthenticated;
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