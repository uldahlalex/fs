using api.Models.Exceptions;
using api.Models.ServerEvents;
using api.StaticHelpers.ExtensionMethods;
using Fleck;
using Serilog;

namespace api.StaticHelpers;

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

        if (exception is JwtVerificationException)
        {
            socket.UnAuthenticate();
            socket.SendDto(new ServerRejectsClientJwt());
        }
        //todo prod and dev env separation
    }
}