using System.Security.Authentication;
using api.ServerEvents;
using core.ExtensionMethods;
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
}