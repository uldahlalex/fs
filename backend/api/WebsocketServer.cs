using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Authentication;
using core.ExtensionMethods;
using core.Models.DbModels;
using core.Models.QueryModels;
using core.Models.WebsocketTransferObjects;
using core.SecurityUtilities;
using core.State;
using Fleck;
using Infrastructure;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace api.Websocket;

public class WebsocketServer(ChatRepository chatRepository, 
    TimeSeriesRepository timeSeriesRepository,
    Mediator mediator)
{
    public void StartWebsocketServer()
    {
        var server = new WebSocketServer("ws://127.0.0.1:8181");
        server.RestartAfterListenError = true;
        server.Start(socket =>
        {
            socket.OnMessage = async message =>
            {
                Log.Information(message, "Client sent message: ");
                var baseTransferObject = message.Deserialize<BaseTransferObject>();
                var eventType = baseTransferObject.eventType;
                try
                {
          
                    var request = new WebSocketRequest 
                    {
                        Socket = socket,
                        RawMessage = message
                    };
                    await mediator.Send(request);
                        
                }
                catch (Exception e)
                {
                    Log.Error(e, "WebsocketServer");
                    var errorMessage =
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Development")
                            ? e.InnerException.Message
                            : "Something went wrong";
                    socket.SendDto(new ServerSendsErrorMessageToClient
                    {
                        errorMessage = errorMessage,
                        receivedEventType = eventType
                    });
                }
            };
            socket.OnOpen = () =>
            {
                socket.AddToWebsocketConnections();
                Log.Information("Connected: " + socket.ConnectionInfo.Id);
            };
            socket.OnClose = () =>
            {
                foreach (var topic in socket.GetMetadata().subscribedToTopics.ToList())
                {
                    WebsocketExtensions.BroadcastObjectToTopicListeners(
                        new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
                            { message = "Client left room: " + topic, user = socket.GetMetadata().userInfo }, topic);
                }


                socket.RemoveFromWebsocketConnections();
                Log.Information("Disconnected: " + socket.ConnectionInfo.Id);
            };
            socket.OnError = exception =>
            {
                Log.Error(exception, "WebsocketServer");
                var errorMessage =
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Development")
                        ? exception.Message
                        : "Something went wrong";
                socket.SendDto(new ServerSendsErrorMessageToClient
                {
                    errorMessage = errorMessage,
                    receivedEventType = "No event type"
                });
            };
        });
    }

    private void ExitIfNotAuthenticated(IWebSocketConnection socket, string receivedEventType)
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

    #region ENTRY POINTS

    [UsedImplicitly]
    public void ClientWantsToAuthenticateWithJwt(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToAuthenticateWithJwt>();
        if (!SecurityUtilities.IsJwtValid(request.jwt!))
        {
           socket.UnAuthenticate();
           return;
        }

        var email = SecurityUtilities.ExtractClaims(request.jwt!)["email"];
        var user = chatRepository.GetUser(email);
        if (user.isbanned)
        {
           socket.UnAuthenticate();
           return;
        }

        socket.Authenticate(user);
        socket.Send(new ServerAuthenticatesUser { jwt = request.jwt }.ToJsonString());
    }

    [UsedImplicitly]
    public void ClientWantsToLoadOlderMessages(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToLoadOlderMessages>();
        ExitIfNotAuthenticated(socket, request.eventType);
        var messages = chatRepository.GetPastMessages(
            request.roomId,
            request.lastMessageId);
        socket.SendDto(new ServerSendsOlderMessagesToClient { messages = messages, roomId = request.roomId });
    }


    [UsedImplicitly]
    public void ClientWantsToSendMessageToRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToSendMessageToRoom>();
        ExitIfNotAuthenticated(socket, request.eventType);
        var insertedMessage =
            chatRepository.InsertMessage(request.roomId, socket.GetMetadata().userInfo.id, request.messageContent!);
        var messageWithUserInfo = new MessageWithSenderEmail()
        {
            room = insertedMessage.room,
            sender = insertedMessage.sender,
            timestamp = insertedMessage.timestamp,
            messageContent = insertedMessage.messageContent,
            id = insertedMessage.id,
            email = socket.GetMetadata().userInfo.email
        };
        WebsocketExtensions.BroadcastObjectToTopicListeners(new ServerBroadcastsMessageToClientsInRoom
        {
            message = messageWithUserInfo,
            roomId = request.roomId,
        }, request.roomId.ToString());
    }


    [UsedImplicitly]
    public void ClientWantsToEnterRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToEnterRoom>();
        ExitIfNotAuthenticated(socket, request.eventType);

        WebsocketExtensions.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
        {
            message = "Client joined the room!",
            user = socket.GetMetadata().userInfo,
            roomId = request.roomId
        }, request.roomId.ToString());
        socket.SubscribeToTopic("ChatRooms/" + request.roomId.ToString());
        socket.SendDto(new ServerAddsClientToRoom
        {
            messages = chatRepository.GetPastMessages(request.roomId),
            liveConnections = socket.CountUsersInRoom(request.roomId.ToString()),
            roomId = request.roomId
        });
    }

    [UsedImplicitly]
    public void ClientWantsToLeaveRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToLeaveRoom>();
        socket.UnsubscribeFromTopic(request.roomId.ToString());
        WebsocketExtensions.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
                { message = "Client left room: " + request.roomId, user = socket.GetMetadata().userInfo },
            request.roomId.ToString());
    }

    [UsedImplicitly]
    public void ClientWantsToRegister(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToRegister>();
        if (chatRepository.UserExists(request.email)) throw new Exception("User already exists!");
        var salt = SecurityUtilities.GenerateSalt();
        var hash = SecurityUtilities.Hash(request.password!, salt);
        var user = chatRepository.InsertUser(request.email!, hash, salt);
        var jwt = SecurityUtilities.IssueJwt(
            new Dictionary<string, object?> { { "email", user.email }, { "id", user.id } });
        socket.Authenticate(user);
        socket.SendDto(new ServerAuthenticatesUser
        {
            jwt = jwt
        });
    }



    [UsedImplicitly]
    public void ClientWantsToSubscribeToTimeSeriesData(IWebSocketConnection socket, string dto)
    {
        socket.SubscribeToTopic("TimeSeries");
        var data = timeSeriesRepository.GetOlderTimeSeriesDataPoints();
        socket.SendDto(new ServerSendsOlderTimeSeriesDataToClient() { timeseries = data });
    }

    #endregion
}