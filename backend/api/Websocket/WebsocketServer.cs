using System.Collections.Concurrent;
using JetBrains.Annotations;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
using System.Security.Authentication;
using core.Attributes;
using core.ExtensionMethods;
using core.Models;
using core.Models.WebsocketTransferObjects;
using core.SecurityUtilities;
using Fleck;
using Force.DeepCloner;
using Infrastructure;
using Newtonsoft.Json;
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
        //todo resp or not
    }

    [UsedImplicitly]
    public void ClientWantsToLoadOlderMessages(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToLoadOlderMessages>();
        ExitIfNotAuthenticated(socket, request.eventType);
        var messages = chatRepository.GetPastMessages(
            request.roomId,
            request.lastMessageId);
        var resp = new ServerSendsOlderMessagesToClient()
            { messages = messages, roomId = request.roomId };
        ServerSendsOlderMessagesToClient(socket, resp);
    }


    [UsedImplicitly]
    public void ClientWantsToSendMessageToRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToSendMessageToRoom>();
        ExitIfNotAuthenticated(socket, request.eventType);
        Message insertedMessage =
            chatRepository.InsertMessage(request.roomId, socket.GetUserIdForConnection(), request.messageContent!);

        ServerBroadcastsMessageToClientsInRoom(new ServerBroadcastsMessageToClientsInRoom()
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
        socket.JoinRoom(request.roomId);
        ServerAddsClientToRoom(socket, new ServerAddsClientToRoom()
        {
            messages = chatRepository.GetPastMessages(request.roomId),
            roomId = request.roomId,
        });

        ServerNotifiesClientsInRoom(new ServerNotifiesClientsInRoom()
        {
            message = "Client joined the room!",
            roomId = request.roomId
        });
    }

    [UsedImplicitly]
    public void ClientWantsToLeaveRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToLeaveRoom>();
        socket.RemoveFromRoom(request.roomId);
        ServerNotifiesClientsInRoom(new ServerNotifiesClientsInRoom() { message = "Client left the room!" });
    }

    [UsedImplicitly]
    public void ClientWantsToRegister(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToRegister>();
        if (chatRepository.UserExists(request.email)) throw new Exception("User already exists!");
        var salt = SecurityUtilities.GenerateSalt();
        var hash = SecurityUtilities.Hash(request.password!, salt);
        EndUser user = chatRepository.InsertUser(request.email!, hash, salt);
        var jwt = SecurityUtilities.IssueJwt(
            new Dictionary<string, object?>() { { "email", user.email }, { "id", user.id } });
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
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>()
            { { "email", user.email }, { "id", user.id } });
        socket.Authenticate(user.id);
        ServerAuthenticatesUser(socket, new ServerAuthenticatesUser(){jwt = jwt});
    }

    #endregion
    

    #region EXIT POINTS

    //Nuværende regel: Alt i client, nedenstående kun til at transmit. En klient kan godt kalde flere transmit funktioner, men en trasnsmit har højst en tramission
    //note skal broadcast laves om til at have et loop?
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


    private void ServerNotifiesClientsInRoom(ServerNotifiesClientsInRoom dto)
    {
        foreach (var connection in LiveSocketConnections)
        {
            if (connection.Value.GetConnectedRooms().Contains(dto.roomId)) ;
            connection.Value.Send(dto.ToJsonString());
        }
    }

    #endregion

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
}