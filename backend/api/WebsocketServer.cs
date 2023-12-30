using System.Globalization;
using System.Security.Authentication;
using api.ClientEventHandlers;
using api.Helpers;
using api.Helpers.ExtensionMethods;
using api.Models;
using api.Models.ServerEvents;
using Fleck;
using Serilog;

namespace api;

public class WebsocketServer(IServiceProvider serviceProvider)
{
    public static HashSet<Type> HandlerTypes = null!;
    
    public void StartWebsocketServer()
    {
        var server = new WebSocketServer("ws://0.0.0.0:8181");
        server.RestartAfterListenError = true;
        server.Start(_config);
    }
    
    private readonly Action<IWebSocketConnection> _config = socket =>
    {
        socket.OnMessage = async message =>
        {
            string eventType = null;
            try
            {
                Console.WriteLine(message);
                eventType = message.DeserializeToModelAndValidate<BaseTransferObject>().eventType;
                Console.WriteLine(eventType);
                var handlerType = HandlerTypes.FirstOrDefault(t => t.Name == eventType);
                if (handlerType != null)
                {
                    // Using dynamic here because the exact handler type is known only at runtime
                    dynamic handler = serviceProvider.GetService(handlerType)!;
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
            socket.RemoveFromWebsocketConnections();
            Log.Information("Disconnected: " + socket.ConnectionInfo.Id);
        };
        socket.OnError = exception =>
        {
            GeneralExceptionHandler.Handle(exception: exception, socket: socket, eventType: null, message: null);
        };
    };


}