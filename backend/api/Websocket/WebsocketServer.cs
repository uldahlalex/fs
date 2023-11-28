using System.Collections.Concurrent;
using JetBrains.Annotations;
using System.Reflection;
using System.Security.Authentication;
using core.ExtensionMethods;
using core.Models;
using core.Models.WebsocketTransferObjects;
using core.SecurityUtilities;
using core.TextTools;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;
using Serilog;

namespace api.Websocket;

public class WebsocketServer(ChatRepository chatRepository)
{
    private readonly ConcurrentDictionary<Guid, IWebSocketConnection> _liveSocketConnections = new(); 
    public void StartWebsocketServer()
    {
        try
        {
            var server = new WebSocketServer("ws://127.0.0.1:8181");
            server.RestartAfterListenError = true;
            server.Start(socket =>
            {
                socket.OnMessage = message =>
                {
                    var eventType = Deserializer<BaseTransferObject>
                        .Deserialize(message)
                        .eventType;
                    try
                    {
                        GetType()
                            .GetMethod(eventType, BindingFlags.Public | BindingFlags.Instance)!
                            .Invoke(this, new object[] { socket, message });
                    }
                    catch (Exception e)
                    {
                        //check that inner exc and stack trace is also logged
                        Log.Error(e, "WebsocketServer");
                        var msg = new ServerSendsErrorMessageToClient()
                        {
                            receivedEventType = eventType,
                            errorMessage = "Could not find correct event!"
                        };
                        socket.Send(JsonConvert.SerializeObject(msg));
                    }
                    

                };
                socket.OnOpen = () =>
                {
                    _liveSocketConnections.TryAdd(socket.ConnectionInfo.Id, socket);
                };
                socket.OnClose = () => { RemoveClientFromConnections(socket); };
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
        catch (Exception e)
        {
            Log.Error(e, "WebsocketServer");
        }
    }

    #region Events
    
    [UsedImplicitly]
    public void ClientWantsToLoadOlderMessages(IWebSocketConnection socket, string dto)
    {
        var request =
            Deserializer<ClientWantsToLoadOlderMessages>.DeserializeToModelAndValidate(dto);
        var messages = chatRepository.GetPastMessages(
            request.roomId,
            request.lastMessageId);
        var resp = new ServerSendsOlderMessagesToClient()
            { messages = messages, roomId = request.roomId };
        socket.Send(JsonConvert.SerializeObject(resp));
    }

    [UsedImplicitly]
    public void ClientWantsToSendMessageToRoom(IWebSocketConnection socket, string dto)
    {
        var request =
            Deserializer<ClientWantsToSendMessageToRoom>.DeserializeToModelAndValidate(dto);
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
        var request = Deserializer<ClientWantsToEnterRoom>.DeserializeToModelAndValidate(dto);
        if (!_liveSocketConnections.ContainsKey(socket.ConnectionInfo.Id))
            return;
        socket.JoinRoom(request.roomId);
        var data = new ServerAddsClientToRoom()
        {
            messages = chatRepository.GetPastMessages(request.roomId),
            roomId = request.roomId,
        };
        socket.Send(JsonConvert.SerializeObject(data));
        BroadcastMessageToRoom(request.roomId, new ServerNotifiesClientsInRoom()
        {
            roomId = request.roomId,
            message = "A new user has entered the room!"
        });
    }

    [UsedImplicitly]
    public void ClientWantsToLeaveRoom(IWebSocketConnection socket, string dto)
    {
        var request = Deserializer<ClientWantsToLeaveRoom>.DeserializeToModelAndValidate(dto);

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
        var request = Deserializer<ClientWantsToRegister>.DeserializeToModelAndValidate(dto);
        if (chatRepository.UserExists(request.email)) throw new Exception("User already exists!");
        var salt = SecurityUtilities.GenerateSalt();
        var hash = SecurityUtilities.Hash(request.password!, salt);
        EndUser user = chatRepository.InsertUser(request.email!, hash, salt);
        var jwt = SecurityUtilities.IssueJwt(
            new Dictionary<string, object?>() { { "email", user.email } });
        socket.Authenticate();
        socket.Send(JsonConvert.SerializeObject(new ServerAuthenticatesUser { jwt = jwt }));
    }

    [UsedImplicitly]
    public void ClientWantsToAuthenticate(IWebSocketConnection socket, string dto)
    {
        var request =
            Deserializer<ClientWantsToAuthenticate>.DeserializeToModelAndValidate(dto);
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
        socket.Send(JsonConvert.SerializeObject(new ServerAuthenticatesUser { jwt = jwt }));
    }
    #endregion

    #region Helpers

    private void BroadcastMessageToRoom(int roomId, BaseTransferObject transferObject)
    {
        foreach (var socketKeyValuePair in _liveSocketConnections)
        {
            if (!socketKeyValuePair.Value.GetConnectedRooms().Contains(roomId))
                throw new KeyNotFoundException("User is not present in the room they are trying to send a message to!");
            try
            {
                var roomMember = _liveSocketConnections.GetValueOrDefault(socketKeyValuePair.Key) ?? throw new Exception("Could not find socket with GUID "+socketKeyValuePair.Key);
                roomMember.Send(JsonConvert.SerializeObject(transferObject));
            }
            catch (Exception e)
            {
                Log.Error(e, "WebsocketUtilities");
            }
        }
    }

    private void RemoveClientFromConnections(IWebSocketConnection socket)
    {
        if (_liveSocketConnections.ContainsKey(socket.ConnectionInfo.Id))
            _liveSocketConnections.Remove(socket.ConnectionInfo.Id, out _);
    }

    #endregion
}