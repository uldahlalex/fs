using System.Collections.Concurrent;
using JetBrains.Annotations;
using System.Reflection;
using System.Security.Authentication;
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
                var eventType =
                    message.Deserialize<BaseTransferObject>()
                        .eventType;
                try
                {
                    GetType()
                        .GetMethod(eventType, BindingFlags.Public | BindingFlags.Instance)!
                        .Invoke(this, new object[] { socket, message });
                }
                catch (Exception e)
                {
                    Log.Error(e, "WebsocketServer");
                    var msg = new ServerSendsErrorMessageToClient()
                    {
                        receivedEventType = eventType,
                        errorMessage =
                            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals("Development")
                                ? e.Message
                                : "Something went wrong"
                    };
                    socket.Send(JsonConvert.SerializeObject(msg));
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
                var data = new ServerSendsErrorMessageToClient()
                {
                    errorMessage = exception.Message
                };
                socket.Send(JsonConvert.SerializeObject(data));
            };
        });
    }

    #region Events

    [UsedImplicitly]
    public void ClientWantsToLoadOlderMessages(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToLoadOlderMessages>();
        var messages = chatRepository.GetPastMessages(
            request.roomId,
            request.lastMessageId);
        var resp = new ServerSendsOlderMessagesToClient()
            { messages = messages, roomId = request.roomId };
        socket.Send(resp.ToJsonString());
    }

    [UsedImplicitly]
    public void ClientWantsToSendMessageToRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToSendMessageToRoom>();
        Message messageToInsert = new Message()
        {
            messageContent = request.messageContent,
            room = request.roomId,
            sender = 1, //todo refactor til at tage fra jwt
            timestamp = DateTimeOffset.UtcNow
        };
        var insertionResponse = chatRepository.InsertMessage(messageToInsert);
        var response = new ServerBroadcastsMessageToClientsInRoom()
        {
            message = insertionResponse,
            roomId = request.roomId
        };

        BroadcastMessageToRoom(request.roomId, response);
    }


    [UsedImplicitly]
    public void ClientWantsToEnterRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToEnterRoom>();
        if (!LiveSocketConnections.ContainsKey(socket.ConnectionInfo.Id))
            return;
        socket.JoinRoom(request.roomId);
        var data = new ServerAddsClientToRoom()
        {
            messages = chatRepository.GetPastMessages(request.roomId),
            roomId = request.roomId,
        };
        socket.Send(data.ToJsonString());
        BroadcastMessageToRoom(request.roomId, new ServerNotifiesClientsInRoom()
        {
            roomId = request.roomId,
            message = "A new user has entered the room!"
        });
    }

    [UsedImplicitly]
    public void ClientWantsToLeaveRoom(IWebSocketConnection socket, string dto)
    {
        var request = dto.DeserializeToModelAndValidate<ClientWantsToLeaveRoom>();
        socket.RemoveFromRoom(request.roomId);
        BroadcastMessageToRoom(request.roomId, new ServerNotifiesClientsInRoom
        {
            message = "A user has left the room!",
            roomId = request.roomId
        });
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
            new Dictionary<string, object?>() { { "email", user.email } });
        socket.Authenticate();
        socket.Send(new ServerAuthenticatesUser { jwt = jwt }.ToJsonString());
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
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>() { { "email", user.email } });
        socket.Authenticate();
        socket.Send(new ServerAuthenticatesUser { jwt = jwt }.ToJsonString());
    }

    #endregion

    #region Helpers

    private void BroadcastMessageToRoom(int roomId, BaseTransferObject transferObject)
    {
        try
        {
            foreach (var socketKeyValuePair in
                     LiveSocketConnections) //dictionary lookup between room and members would probably be faster, but whatever
            {
                if (!socketKeyValuePair.Value.GetConnectedRooms().Contains(roomId)) continue; //continue skips to next iteration
                LiveSocketConnections
                    .GetValueOrDefault(socketKeyValuePair.Key)!
                    .Send(transferObject.ToJsonString());
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "WebsocketUtilities");
        }
    }

    private void ClientFailsAuthentication(IWebSocketConnection socket)
    {
        
        //skal måske bare kaldes hvor den skal bruges i stedet for at have en wrapper metode
        socket.UnAuthenticate();
    }

    #endregion
}