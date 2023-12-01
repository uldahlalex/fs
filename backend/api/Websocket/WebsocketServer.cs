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
                Log.Information(message);
                var eventType =
                    message.Deserialize<BaseTransferObject>()
                    .eventType;
                try
                {
                    //Invoke client requests based on relection of eventType
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
             /*   var clone = socket.DeepClone();
                string json = JsonConvert.SerializeObject(clone, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    Converters = new List<JsonConverter> { new SafeConverter() }
                });
                Log.Information(json);*/
             //todo hvis de er tid lav custom serializer
             //todo pt bare find ud af hvorfor test klienten ikke bliver føjet til listen,
             //mens browser klienten gør
             Log.Information("Starts to connect!");
                LiveSocketConnections.TryAdd(socket.ConnectionInfo.Id, socket);
                Log.Information("connected: "+socket.ConnectionInfo.Id);
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
        socket.Send(JsonConvert.SerializeObject(resp));
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
        socket.Send(JsonConvert.SerializeObject(new ServerAuthenticatesUser { jwt = jwt }));
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
        socket.Send(JsonConvert.SerializeObject(new ServerAuthenticatesUser { jwt = jwt }));
    }

    #endregion

    #region Helpers

    private void BroadcastMessageToRoom(int roomId, BaseTransferObject transferObject)
    {
        //dictionary lookup between room and members would probably be faster
        foreach (var socketKeyValuePair in LiveSocketConnections)
        {
            //todo here is the bug
            if (socketKeyValuePair.Value.GetConnectedRooms().Contains(roomId))
                //throw new KeyNotFoundException("User is not present in the room they are trying to send a message to! Socket ID: "+socketKeyValuePair.Key);
            try
            {
                var roomMember = LiveSocketConnections.GetValueOrDefault(socketKeyValuePair.Key) ??
                                 throw new Exception("Could not find socket with GUID " + socketKeyValuePair.Key);
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
        if (LiveSocketConnections.ContainsKey(socket.ConnectionInfo.Id))
            LiveSocketConnections.Remove(socket.ConnectionInfo.Id, out _);
    }

    #endregion
}