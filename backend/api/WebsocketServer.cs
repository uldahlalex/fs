using System.Security.Authentication;
using api.ClientEventHandlers;
using api.Exceptions;
using api.ExtensionMethods;
using api.Helpers;
using api.Models;
using api.Models.ServerEvents;
using Fleck;
using MediatR;
using Serilog;

namespace api;

public class WebsocketServer(Mediator mediator, EventHandlerService eventHandlerService)
{
    private readonly Action<IWebSocketConnection> config = socket =>
    {
        socket.OnMessage = async message =>
        {
            string eventType = null;
            try
            {
                Log.Information(message, "Client sent message: ");
                eventType = message.DeserializeToModelAndValidate<BaseTransferObject>().eventType;
                await eventHandlerService.HandleEventAsync(eventType, message, socket);

            }
            catch (AuthenticationException exception)
            {
                socket.UnAuthenticate();
                GeneralExceptionHandler.Handle(exception, socket, eventType,
                    message);
            }
            catch (Exception exception)
            {
                GeneralExceptionHandler.Handle(exception, socket, eventType,
                    message);
            }
        };

        socket.OnOpen = () =>
        {
            socket.AddToWebsocketConnections();
            Log.Information("Added to connection pool: " + socket.ConnectionInfo.Id);
        };
        socket.OnClose = () =>
        {
            //Subscribe to topics and broadcast to topic listeners that someone has left the room and remove conn from pool


            socket.RemoveFromWebsocketConnections();
            Log.Information("Disconnected: " + socket.ConnectionInfo.Id);
        };
        socket.OnError = exception =>
        {
            Log.Error(exception, "WebsocketServer");
            socket.SendDto(new ServerSendsErrorMessageToClient
            {
                errorMessage = exception.Message
            });
        };
    };

    public void StartWebsocketServer()
    {
        var server = new WebSocketServer("ws://127.0.0.1:8181");
        server.RestartAfterListenError = true;
        server.Start(config);
    }
}