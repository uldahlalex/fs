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

public class WebsocketServer(Mediator mediator)
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
                var eventTypeRequestMappings =
                    new Dictionary<string,
                            Func<string, IWebSocketConnection, IRequest>> //Anti-reflection way of invoking
                        {
                            {
                                "ClientWantsToAuthenticate", RequestFactory.CreateRequest<ClientWantsToAuthenticate>
                            },
                            { "ClientWantsToEnterRoom", RequestFactory.CreateRequest<ClientWantsToEnterRoom> },
                            {
                                "ClientWantsToSendMessageToRoom",
                                RequestFactory.CreateRequest<ClientWantsToSendMessageToRoom>
                            }
                        };
                if (eventTypeRequestMappings.TryGetValue(eventType,
                        out var createRequestFunc))
                {
                    var request = createRequestFunc(message, socket);
                    await mediator.Send(request);
                }
                else
                {
                    throw new MediationException("Could not find a valid event to mediate to");
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
        server.Start(config);
    }
}