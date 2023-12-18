using System.Collections.Concurrent;
using System.Security.Authentication;
using api.Helpers.ExtensionMethods;
using api.Models.Enums;
using api.Models.ServerEvents;
using api.State;
using Fleck;

namespace api.Helpers;

public static class WebsocketHelpers
{
    public static void ExitIfNotAuthenticated(IWebSocketConnection socket, string receivedEventType)
    {
        if (WebsocketConnections.ConnectionPool.ContainsKey(socket.ConnectionInfo.Id) &&
            socket.GetMetadata().IsAuthenticated)
            return; //OK

        socket.SendDto(new ServerSendsErrorMessageToClient
        {
            receivedEventType = receivedEventType,
            errorMessage = "Unauthorized access."
        });
        throw new AuthenticationException("Unauthorized access.");
    }

    public static void SubscribeToTopic(Guid connectionId, TopicEnums topic)
    {
        if (!WebsocketConnections.TopicSubscriptions.ContainsKey(topic))
            WebsocketConnections.TopicSubscriptions.TryAdd(topic, new ConcurrentBag<Guid>());

        WebsocketConnections.TopicSubscriptions[topic].Add(connectionId);
    }

    public static void UnsubscribeFromTopic(Guid connectionId, TopicEnums topic)
    {
        if (WebsocketConnections.TopicSubscriptions.ContainsKey(topic))
        {
            var bag = WebsocketConnections.TopicSubscriptions[topic];
            var newBag = new ConcurrentBag<Guid>(bag.Where(id => id != connectionId));
            WebsocketConnections.TopicSubscriptions[topic] = newBag;
        }
    }

    public static void BroadcastObjectToTopicListeners(object dto, TopicEnums topic)
    {
        if (WebsocketConnections.TopicSubscriptions.TryGetValue(topic, out var connections))
            foreach (var connectionId in connections)
                if (WebsocketConnections.ConnectionPool.TryGetValue(connectionId, out var socketMetadata))
                {
                    Console.WriteLine(socketMetadata.Socket!.ConnectionInfo.Id);
                    socketMetadata.Socket!.Send(dto.ToJsonString());
                }
    }
}