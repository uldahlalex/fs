using System.Collections.Concurrent;
using core;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace api;

public class FleckServer(
    AuthUtilities auth,
    ChatRepository chatRepository)
{
    private readonly Dictionary<Guid, IWebSocketConnection> _allSockets = new(); //ext method refactor

    private readonly ConcurrentDictionary<int, List<Guid>> _socketsConnectedToRoom = new(
        new List<KeyValuePair<int, List<Guid>>>
        {
            new(1, new List<Guid>()),
            new(2, new List<Guid>()),
            new(3, new List<Guid>()),
        });
    
    //ext method refactor
    public void Start()
    {
        try
        {
            var server = new WebSocketServer("ws://127.0.0.1:8181");
            server.RestartAfterListenError = true;
            server.Start(socket =>
            {
                socket.OnMessage = message => HandleClientMessage(socket, message);
                socket.OnOpen = () =>
                {
                    _allSockets.TryAdd(socket.ConnectionInfo.Id, socket);
                };
                socket.OnClose = () => { PurgeClient(socket); };
            });
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e);
        }
    }


    private void HandleClientMessage(IWebSocketConnection socket, string incomingClientMessagePayload)
    {
        string eventType = Deserializer<BaseTransferObject>.Deserialize(incomingClientMessagePayload).eventType;
        switch (eventType)
        {
            case "ClientWantsToEnterRoom":
                ClientWantsToEnterRoom(socket,
                    Deserializer<ClientWantsToEnterRoom>.Deserialize(incomingClientMessagePayload));
                break;
            case "ClientWantsToSendMessageToRoom":
                ClientWantsToSendMessageToRoom(socket,
                    Deserializer<ClientSendsMessageToRoom>.Deserialize(incomingClientMessagePayload));
                break;
            
            //todo data validation and eventType not found
        }
    }

    private void ClientWantsToSendMessageToRoom(IWebSocketConnection socket,
        ClientSendsMessageToRoom clientSendsMessageToRoom)
    {
        Message messageToInsert = new Message()
        {
            messageContent = clientSendsMessageToRoom.messageContent,
            room = clientSendsMessageToRoom.roomId,
            sender = 1, //todo refactor til at tage fra jwt
            timestamp = DateTimeOffset.UtcNow
        };
        var insertionResponse = chatRepository.InsertMessage(messageToInsert);
        var response = new ServerBroadcastsMessageToClients()
        {
            message = insertionResponse
        };

        BroadcastMessageToRoom(clientSendsMessageToRoom.roomId, response);
    }

    public void BroadcastMessageToRoom(int roomId, BaseTransferObject transferObject)
    {
        foreach (var socketGuid in _socketsConnectedToRoom[roomId])
        {
            var exp = _allSockets.TryGetValue(socketGuid, out var socketToSendTo)
                ? socketToSendTo
                : throw new Exception("Socket not found");
            socketToSendTo.Send(JsonConvert.SerializeObject(transferObject));
        }
    } 

    private void ClientWantsToEnterRoom(IWebSocketConnection socket, ClientWantsToEnterRoom clientWantsToEnterRoom)
    {
        if (!_allSockets.ContainsKey(socket.ConnectionInfo.Id))
            return;
        if (_socketsConnectedToRoom[clientWantsToEnterRoom.roomId].Contains(socket.ConnectionInfo.Id))
            return;
        _socketsConnectedToRoom[clientWantsToEnterRoom.roomId].Add(socket.ConnectionInfo.Id);
        var data = new ServerLetsClientEnterRoom()
        {
            recentMessages = chatRepository.GetPastMessages(),
            roomId = clientWantsToEnterRoom.roomId,
        };
        socket.Send(JsonConvert.SerializeObject(data));
        //send to all other in room
    }
    
    private void ClientWantsToLeaveRoom(IWebSocketConnection socket, ClientWantsToLeaveRoom clientWantsToLeaveRoom)
    {
        if (!_socketsConnectedToRoom[clientWantsToLeaveRoom.roomId].Contains(socket.ConnectionInfo.Id))
            return;
        _socketsConnectedToRoom[clientWantsToLeaveRoom.roomId].Remove(socket.ConnectionInfo.Id);
        //notify people in room
    }
    
    private void PurgeClient(IWebSocketConnection socket)
    {
        foreach (var room in _socketsConnectedToRoom)
        {
            if (room.Value.Contains(socket.ConnectionInfo.Id))
                room.Value.Remove(socket.ConnectionInfo.Id);
        }
        {
            if (_allSockets.ContainsKey(socket.ConnectionInfo.Id))
                _allSockets.Remove(socket.ConnectionInfo.Id, out _);
        }
    }
}



public class AuthUtilities
{
    public void VerifyJwt(IWebSocketConnection socket)
    {
        throw new NotImplementedException();
    }
}

public static class Deserializer<T>
{
    public static T Deserialize(string message)
    {
        return JsonSerializer.Deserialize<T>(message,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new InvalidOperationException(@$"Could not deserialize into {nameof(T)}");
    }
}

public static class ExceptionHandler
{
    public static void Handle(Exception e)
    {
        if (e is DeserializationException deserializationException)
        {
            deserializationException.Handle();
        }
        //others
    }
}

public abstract class DeserializationException(string message, IWebSocketConnection socket, ILogger<object> logger)
    : Exception(message)
{
    public void Handle()
    {
        var data = new ServerSendsErrorMessageToClient()
        {
            errorMessage = Message
        };
        socket.Send(JsonConvert.SerializeObject(data));
        logger.LogError(data.errorMessage);
    }
}


public static class WebSocketExtensions
{
    private static readonly Dictionary<IWebSocketConnection, bool> AuthStates = new();

    public static void SetAuthentication(this IWebSocketConnection connection, bool isAuthenticated)
    {
        AuthStates[connection] = isAuthenticated;
    }

    public static bool IsAuthenticated(this IWebSocketConnection connection)
    {
        return AuthStates.TryGetValue(connection, out var isAuthenticated) && isAuthenticated;
    }
}

