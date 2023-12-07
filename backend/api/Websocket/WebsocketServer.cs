using System.Collections.Concurrent;
using System.Reflection;
using System.Security.Authentication;
using core.ExtensionMethods;
using core.Models;
using core.Models.WebsocketTransferObjects;
using core.SecurityUtilities;
using Fleck;
using Infrastructure;
using JetBrains.Annotations;
using Serilog;

namespace api.Websocket;

public class WebsocketServer(ChatRepository chatRepository)
{
    public readonly ConcurrentDictionary<Guid, IWebSocketConnection> LiveSocketConnections = new();

    public void StartWebsocketServer()
    {
        var server = new WebSocketServer("ws://127.0.0.1:8181");
        server.RestartAfterListenError = true;
        server.Start(socket =>
        {
            socket.OnMessage = message =>
            {
                Log.Information(message, "Client sent message: ");
                var eventType = message.Deserialize<BaseTransferObject>().eventType;
                try
                {
                    GetType()
                        .GetMethod(eventType, BindingFlags.Public | BindingFlags.Instance)!
                        .Invoke(this, new object[] { socket, message });
                }
                catch (Exception e)
                {
                    Log.Error(e, "WebsocketServer");
                    var errorMessage =
                        Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Development")
                            ? e.Message
                            : "Something went wrong";
                    var dto = new ServerSendsErrorMessageToClient
                    {
                        errorMessage = errorMessage,
                        receivedEventType = eventType
                    };
                    ServerSendsErrorMessageToClient(socket, dto);
                }
            };
            socket.OnOpen = () =>
            {
                LiveSocketConnections.TryAdd(socket.ConnectionInfo.Id, socket);
                Log.Information("Connected: " + socket.ConnectionInfo.Id);
            };
            socket.OnClose = () =>
            {
                foreach (var connectedRoom in socket.GetConnectedRooms())
                    ServerNotifiesClientsInRoom(new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
                        { message = "Client left the room!", roomId = connectedRoom });

                if (LiveSocketConnections.ContainsKey(socket.ConnectionInfo.Id))
                    LiveSocketConnections.Remove(socket.ConnectionInfo.Id, out _);
                Log.Information("Disconnected: " + socket.ConnectionInfo.Id);
            };
            socket.OnError = exception =>
            {
                Log.Error(exception, "WebsocketServer");
                var errorMessage =
                    Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Development")
                        ? exception.Message
                        : "Something went wrong";
                var dto = new ServerSendsErrorMessageToClient
                {
                    errorMessage = errorMessage,
                    receivedEventType = "No event type"
                };
                ServerSendsErrorMessageToClient(socket, dto);
            };
        });
    }

    #region Helpers

    private void ExitIfNotAuthenticated(IWebSocketConnection socket, string receivedEventType)
    {
        if (!socket.IsAuthenticated() && LiveSocketConnections.ContainsKey(socket.ConnectionInfo.Id))
        {
            ServerSendsErrorMessageToClient(socket, new ServerSendsErrorMessageToClient
            {
                receivedEventType = receivedEventType,
                errorMessage = "Unauthorized access."
            });
            throw new AuthenticationException("Unauthorized access.");
        }
    }

    #endregion

    #region ENTRY POINTS

    [UsedImplicitly]
    public void ClientWantsToAuthenticateWithJwt(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToAuthenticateWithJwt>();
        if (SecurityUtilities.IsJwtValid(request.jwt!) &&
            !chatRepository.IsUserBanned(SecurityUtilities.ExtractClaims(request.jwt!)["email"]))
        {
            var id = int.Parse(SecurityUtilities.ExtractClaims(request.jwt!)["id"]);
            socket.Authenticate(id);
        }
        else
        {
            socket.UnAuthenticate();
        }

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
        var resp = new ServerSendsOlderMessagesToClient { messages = messages, roomId = request.roomId };
        ServerSendsOlderMessagesToClient(socket, resp);
    }


    [UsedImplicitly]
    public void ClientWantsToSendMessageToRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToSendMessageToRoom>();
        ExitIfNotAuthenticated(socket, request.eventType);
        var insertedMessage =
            chatRepository.InsertMessage(request.roomId, socket.GetUserIdForConnection(), request.messageContent!);

        ServerBroadcastsMessageToClientsInRoom(new ServerBroadcastsMessageToClientsInRoom
        {
            message = insertedMessage,
            roomId = request.roomId
        });
    }


    [UsedImplicitly]
    public void ClientWantsToEnterRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToEnterRoom>();
        ExitIfNotAuthenticated(socket, request.eventType);

        ServerNotifiesClientsInRoom(new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
        {
            message = "Client joined the room!",
            roomId = request.roomId
        });
        socket.JoinRoom(request.roomId);
        ServerAddsClientToRoom(socket, new ServerAddsClientToRoom
        {
            messages = chatRepository.GetPastMessages(request.roomId),
            liveConnections = LiveSocketConnections.Values.Where(x => x.GetConnectedRooms().Contains(request.roomId))
                .Select(x => x.ConnectionInfo.Id).Count(),
            roomId = request.roomId
        });
    }

    [UsedImplicitly]
    public void ClientWantsToLeaveRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToLeaveRoom>();
        socket.RemoveFromRoom(request.roomId);
        ServerNotifiesClientsInRoom(new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
            { message = "Client left the room!" });
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
        socket.Authenticate(user.id);
        ServerAuthenticatesUser(socket, new ServerAuthenticatesUser
        {
            jwt = jwt
        });
    }

    [UsedImplicitly]
    public void ClientWantsToAuthenticate(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToAuthenticate>();
        EndUser user;
        try
        {
            user = chatRepository.GetUser(request.email!);
        }
        catch (Exception e)
        {
            Log.Error(e, "WebsocketServer");
            throw new AuthenticationException("User does not exist!");
        }

        var expectedHash = SecurityUtilities.Hash(request.password!, user.salt!);
        if (!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong password!");
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>
            { { "email", user.email }, { "id", user.id } });
        socket.Authenticate(user.id);
        ServerAuthenticatesUser(socket, new ServerAuthenticatesUser { jwt = jwt });
    }

    #endregion


    #region EXIT POINTS

    //Note: everything is very single-purpose. Sometimes lines are duplicated in order to not share logic
    private void ServerAddsClientToRoom(IWebSocketConnection socket, ServerAddsClientToRoom dto)
    {
        socket.Send(dto.ToJsonString());
    }

    private void ServerAuthenticatesUser(IWebSocketConnection socket, ServerAuthenticatesUser dto)
    {
        socket.Send(dto.ToJsonString());
    }

    private void ServerBroadcastsMessageToClientsInRoom(ServerBroadcastsMessageToClientsInRoom dto)
    {
        foreach (var connection in LiveSocketConnections)
        {
            if (connection.Value.GetConnectedRooms().Contains(dto.roomId)) ;
            connection.Value.Send(dto.ToJsonString());
        }
    }


    private void ServerSendsErrorMessageToClient(IWebSocketConnection socket, ServerSendsErrorMessageToClient dto)
    {
        socket.Send(dto.ToJsonString());
    }

    private void ServerSendsOlderMessagesToClient(IWebSocketConnection socket, ServerSendsOlderMessagesToClient dto)
    {
        socket.Send(dto.ToJsonString());
    }


    /**
     * THIS ONE IS MULTI PURPOSE
     */
    private void ServerNotifiesClientsInRoom(ServerNotifiesClientsInRoom dto)
    {
        foreach (var connection in LiveSocketConnections)
        {
            if (connection.Value.GetConnectedRooms().Contains(dto.roomId)) ;
            connection.Value.Send(dto.ToJsonString());
        }
    }

    #endregion
}