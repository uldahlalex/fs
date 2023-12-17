using System.Security.Authentication;
using api.ClientEvents;
using api.Resusables;
using api.ServerEvents;
using api.SharedApiModels;
using core.Exceptions;
using core.ExtensionMethods;
using core.State;
using Fleck;
using MediatR;
using Serilog;

namespace api;

public class WebsocketServer(Mediator mediator)
{
    public void StartWebsocketServer()
    {
        var server = new WebSocketServer("ws://127.0.0.1:8181");
        server.RestartAfterListenError = true;
        server.Start(config);
    }

    private Action<IWebSocketConnection> config = socket =>
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
                                { "ClientWantsToEnterRoom", RequestFactory.CreateRequest<ClientWantsToEnterRoom> }
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
                    GeneralExceptionHandler.Handle(exception: exception, socket: socket, eventType: eventType,
                        message: message);
                }
                catch (Exception exception)
                {
                    GeneralExceptionHandler.Handle(exception: exception, socket: socket, eventType: eventType,
                        message: message);
                }
            };

            socket.OnOpen = () =>
            {
                socket.AddToWebsocketConnections();
                Log.Information("Added to connection pool: " + socket.ConnectionInfo.Id);
            };
            socket.OnClose = () =>
            {
                foreach (var topic in socket.GetMetadata().subscribedToTopics.ToList())
                    Reusables.BroadcastObjectToTopicListeners(
                        new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
                            { user = socket.GetMetadata().userInfo }, topic);


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
}