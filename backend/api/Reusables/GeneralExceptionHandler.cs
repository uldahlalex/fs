using api.ExtensionMethods;
using api.ServerEvents;
using Fleck;
using Serilog;

namespace api.Reusables;

public static class GeneralExceptionHandler
{
    public static void Handle(Exception exception, IWebSocketConnection socket, string? eventType, string? message)
    {
        Log.Error(exception, "WebsocketServer");
        socket.SendDto(new ServerSendsErrorMessageToClient
        {
            receivedEventType = eventType ?? "Could not determine event type from: \n" + message,
            errorMessage = exception.Message
        });
    }
}