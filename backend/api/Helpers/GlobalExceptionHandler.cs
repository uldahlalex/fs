using api.Extensions;
using api.Models.ServerEvents;
using Fleck;
using Serilog;

namespace api.Helpers;

public static class GlobalExceptionHandler
{
    public static void Handle(this Exception exception, IWebSocketConnection socket, string? message)
    {
        Log.Error(exception, "Global exception handler");
        socket.SendDto(new ServerSendsErrorMessageToClient
        {
            receivedMessage = message,
            errorMessage = exception.Message
        });
        //todo prod and dev env separation
    }
}