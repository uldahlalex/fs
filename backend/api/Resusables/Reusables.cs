using System.Security.Authentication;
using api.ServerEvents;
using core.ExtensionMethods;
using core.State;
using Fleck;

namespace api.Resusables;

public static class Reusables
{
    public static void ExitIfNotAuthenticated(IWebSocketConnection socket, string receivedEventType)
    {
        if (socket.GetMetadata().isAuthenticated && socket.IsInWebsocketConnections())
            return;

        socket.SendDto(new ServerSendsErrorMessageToClient
        {
            receivedEventType = receivedEventType,
            errorMessage = "Unauthorized access."
        });
        throw new AuthenticationException("Unauthorized access.");
    }

    public static void BroadcastObjectToTopicListeners(object dto, string topic)
    {
        foreach (var socket in WebsocketConnections.ConnectionPool.Values.Where(x => x.subscribedToTopics.Contains(topic)))
        {
            Console.WriteLine(socket.socket!.ConnectionInfo.Id);
            socket.socket!.Send(dto.ToJsonString());
        }
    }
}