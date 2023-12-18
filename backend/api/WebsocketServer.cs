using System.Security.Authentication;
using api.ClientEventHandlers;
using api.ExtensionMethods;
using api.Helpers;
using api.Models;
using api.Models.ServerEvents;
using Fleck;
using Serilog;

namespace api;

public class WebsocketServer(IServiceProvider serviceProvider)
{

    private static readonly List<Type> HandlerTypes = new()
    {
        typeof(ClientWantsToAuthenticate),
        typeof(ClientWantsToEnterRoom)
    };
    
    private readonly Action<IWebSocketConnection> _config = socket =>
    {
        socket.OnMessage = async message =>
        {
            string eventType = null;
            try
            {
                eventType = message.DeserializeToModelAndValidate<BaseTransferObject>().eventType;
                var handlerType = HandlerTypes.FirstOrDefault(t => t.Name == eventType);
                if (handlerType != null)
                {
                    // Using dynamic here because the exact handler type is known only at runtime
                    dynamic handler = serviceProvider.GetRequiredService(handlerType);
                    await handler.DeserializeAndInvokeHandler(message, socket);
                }
                else
                {
                    throw new InvalidOperationException($"Handler not found for event type: {eventType}");
                }
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
        server.Start(_config);
    }
}