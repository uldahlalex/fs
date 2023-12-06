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
                    ServerSendsErrorMessageToClient(socket, errorMessage, eventType);
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
                ServerSendsErrorMessageToClient(socket, errorMessage, "No event type");
            };
        });
    }

    #region ClientWantsTo

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

    }

    [UsedImplicitly]
    public void ClientWantsToLoadOlderMessages(IWebSocketConnection socket, string dto)
    {
        
        var request = dto.DeserializeToModelAndValidate<ClientWantsToLoadOlderMessages>();
        ExitIfNotAuthenticated(socket, request.eventType);
        ServerSendsOlderMessagesToClient(socket, request.roomId, request.lastMessageId);
    }
    

    [UsedImplicitly]
    public void ClientWantsToSendMessageToRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToSendMessageToRoom>();
        ExitIfNotAuthenticated(socket, request.eventType);
        Message insertedMessage = chatRepository.InsertMessage(request.roomId, socket.GetUserIdForConnection(), request.messageContent!);
        ServerBroadcastsMessageToClientsInRoom(insertedMessage, request.roomId);
    }



    [UsedImplicitly]
    public void ClientWantsToEnterRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToEnterRoom>();
        ExitIfNotAuthenticated(socket, request.eventType);
        ServerAddsClientToRoom(socket, request.roomId);
        ServerNotifiesClientsInRoom( request.roomId, "A new user has entered the room!");
    }

    [UsedImplicitly]
    public void ClientWantsToLeaveRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToLeaveRoom>();
        socket.RemoveFromRoom(request.roomId);
        ServerNotifiesClientsInRoom(request.roomId, "A user has left the room!");
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
            new Dictionary<string, object?>() { { "email", user.email }, {"id", user.id} });
        socket.Authenticate(user.id);
        ServerAuthenticatesUser(socket, jwt);
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
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>() { { "email", user.email }, {"id", user.id } });
        socket.Authenticate(user.id);
        socket.Send(new ServerAuthenticatesUser { jwt = jwt }.ToJsonString());
    }

    #endregion
    
    #region ServerSendsData
    
    //pt er disse metoder at tage rå data og sende til klienter. Skal det gøre endnu mere minimalt ved at init objekter ved kalderen eller her?
    //alt: Lave client "henteren" tynd og store metoder her. Spm til dette:
    // - Hvordan ser jeg hvilke features der invoker dette?
    
    
    //Hele rationalet i at separere dette: At kunne se præcis hvad der sender data og hvad der opsamler data. Hvis det er svært at vide hvor logikken skal være,
    //skal der evt være et mellem-lag som tager client DTO -> server DTO? dette kunne være en meget uniform løsning?
    
    private void ServerAddsClientToRoom(IWebSocketConnection socket, int roomId)
    {
        socket.JoinRoom(roomId);
        var data = new ServerAddsClientToRoom()
        {
            messages = chatRepository.GetPastMessages(roomId),
            roomId = roomId,
        };
        socket.Send(data.ToJsonString());
    }
    
    private void ServerSendsErrorMessageToClient(IWebSocketConnection socket, string errorMessage, string receivedEventType)
    {
        var response = new ServerSendsErrorMessageToClient
        {
            errorMessage = errorMessage,
            receivedEventType = receivedEventType
        };
        socket.Send(response.ToJsonString());
    }
    private void ServerSendsOlderMessagesToClient(IWebSocketConnection socket, int roomId, int lastMessageId)
    {
        var messages = chatRepository.GetPastMessages(
            roomId,
            lastMessageId);
        var resp = new ServerSendsOlderMessagesToClient()
            { messages = messages, roomId = roomId };
        socket.Send(resp.ToJsonString());
    }
    
    private void ServerBroadcastsMessageToClientsInRoom(Message message, int roomId)
    {
        var response = new ServerBroadcastsMessageToClientsInRoom()
        {
            message = message,
            roomId = roomId
        };

        foreach (var socket in LiveSocketConnections)
        {
            if (socket.Value.GetConnectedRooms().Contains(roomId))
                socket.Value.Send(response.ToJsonString());
        }
    }
    private void ServerNotifiesClientsInRoom(int roomId, string message)
    {
        var response = new ServerNotifiesClientsInRoom()
        {
            message = message,
            roomId = roomId
        };

        foreach (var socket in LiveSocketConnections)
        {
            if (socket.Value.GetConnectedRooms().Contains(roomId))
                socket.Value.Send(response.ToJsonString());
        }
    }
    
    private void ServerAuthenticatesUser(IWebSocketConnection socket, string jwt)
    {
        var response = new ServerAuthenticatesUser
        {
            jwt = jwt
        };
        socket.Send(response.ToJsonString());
    }

    
    #endregion

    #region Helpers


    
    private void ExitIfNotAuthenticated(IWebSocketConnection socket, string receivedEventType)
    {
        if (!socket.IsAuthenticated() && LiveSocketConnections.ContainsKey(socket.ConnectionInfo.Id)) 
        {
            var response = new ServerSendsErrorMessageToClient
            {
                receivedEventType = receivedEventType,
                errorMessage = "Unauthorized access."
            };
            socket!.Send(response.ToJsonString());
            throw new AuthenticationException("Unauthorized access.");
        }
    }

    #endregion
}