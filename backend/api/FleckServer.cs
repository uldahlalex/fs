using System.Collections.Concurrent;
using core;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace api;

public abstract class FleckServer(
    AuthUtilities auth,
    ChatRepository chatRepository, 
    ILogger<FleckServer> logger)
{
    private readonly ConcurrentDictionary<string, List<IWebSocketConnection>> _clientSocketConnections = new(); //the key is the room name and the value is the sockets in there
    //måske skal dette ændres, således at det er en liste af sockets og tilhørende metadata omkring hver socket

    public void Start()
    {
        try
        {
            var server = new WebSocketServer("ws://127.0.0.1:8181");
            server.RestartAfterListenError = true;
            server.Start(socket =>
            {
                socket.OnMessage = message => HandleClientMessage(socket, message);
                socket.OnOpen = () => { auth.VerifyJwt(socket); };
                socket.OnClose = () => { RemoveClientFromRooms(socket); };
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
                ClientWantsToEnterRoom(socket, Deserializer<ClientWantsToEnterRoom>.Deserialize(incomingClientMessagePayload));
                break;
            case "ClientWantsToSendMessageToRoom":
                ClientWantsToSendMessageToRoom(socket, Deserializer<ServerBroadcastsMessageToClients>.Deserialize(incomingClientMessagePayload));
                break;
        }
    }

    private void ClientWantsToSendMessageToRoom(IWebSocketConnection socket, object deserialized)
    {
        
        // todo refactor
        Message messageToInsert = new Message()
        {
            messageContent = message.messageContent,
            room = message.room,
            sender = message.sender,
            timestamp = DateTimeOffset.UtcNow
        };
        var insertionResponse = new List<Message> { chatRepository.InsertMessage(messageToInsert!) };

        foreach (var s in _clientSocketConnections[socket.ConnectionInfo.Path])
        {
            s.Send(JsonConvert.SerializeObject(insertionResponse));
        }
    
        throw new NotImplementedException();
    }

    private void ClientWantsToEnterRoom(IWebSocketConnection socket, ClientWantsToEnterRoom clientWantsToEnterRoom)
    {
        var data = new ServerLetsClientEnterRoom()
        {
            messages = chatRepository.GetPastMessages(),
            roomId = clientWantsToEnterRoom.roomId,
            eventType = "ServerLetsClientEnterRoom"
        };
        socket.Send(JsonConvert.SerializeObject(data));
    }
    

    public Task AddSocketToRoomConnections(IWebSocketConnection socket)
    {
        //todo refactor
        
        
        
        if (!_clientSocketConnections.ContainsKey(room))
            _clientSocketConnections[room] = new List<IWebSocketConnection>();
        _clientSocketConnections[room].Add(socket);
        Console.WriteLine("A client has entered the room!");
        //return BroadCastToRoom(room, "A client has entered the room!");
        return Task.CompletedTask;
    }

    private Task RemoveClientFromRooms(IWebSocketConnection webSocket)
    {
        // todo refactor til at fjerne fra alle rooms
        if (!_clientSocketConnections.ContainsKey(room))
            return null;
        _clientSocketConnections[room].Remove(webSocket);
        Console.WriteLine("A client has left the room!");
        //return BroadCastToRoom(room, "A client has left the room!");
        return Task.CompletedTask;
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
            eventType = "ServerSendsErrorMessageToClient",
            errorMessage = Message
        };
        socket.Send(JsonConvert.SerializeObject(data));
        logger.LogError(data.errorMessage);
    }
}