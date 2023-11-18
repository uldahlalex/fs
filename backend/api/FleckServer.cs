using System.Collections.Concurrent;
using core;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace api;

public class FleckServer(ChatRepository chatRepository)
{
    private ConcurrentDictionary<string, List<IWebSocketConnection>> socketConnections = new();

    public void Start()
    {
        var server = new WebSocketServer("ws://127.0.0.1:8181");
        server.RestartAfterListenError = true;
        server.Start(socket =>
        {
            socket.OnMessage = message => GetIncomingMessage(socket, message);
            socket.OnOpen = () =>
            {

                VerifyJwt(socket);
            };
            socket.OnClose = () => { RemoveClientFromRooms(socket); };
        });
    }

 

    private void GetIncomingMessage(IWebSocketConnection socket, string message)
    {
        try
        {
            TransferObject deserialized = JsonConvert.DeserializeObject<TransferObject>(message) ??
                                          throw new InvalidOperationException(
                                              "Could not deserialize into a TransferObject");
            switch (deserialized.eventType)
            {
                case "UpstreamEnterRoom":
                    UpstreamEnterRoom(socket, JsonConvert.DeserializeObject<UpstreamEnterRoom>(message)!);
                    break;
                case "UpstreamSendMessage":
                    UpstreamSendMessageToRoom(socket, deserialized);
                    break;
            }
        }
        catch (InvalidOperationException e)
        {
            var data = new DownstreamError()
            {
                eventType = "DownstreamError",
                data = e.Message
            };
            socket.Send("Could not deserialize into a TransferObject");
        }

    }

    private void UpstreamSendMessageToRoom(IWebSocketConnection socket, object deserialized)
    {
        throw new NotImplementedException();
    }

    private void UpstreamEnterRoom(IWebSocketConnection socket, TransferObject deserialized)
    {
        UpstreamEnterRoom upsteamMessage = JsonConvert.DeserializeObject<UpstreamEnterRoom>(JsonConvert.SerializeObject(deserialized.data));
        var data = new UpstreamSendPastMessagesForRoom()
        {
            messages = chatRepository.GetPastMessages(),
            roomId = upsteamMessage.roomId,
            eventType = "DownstreamSendPastMessagesForRoom"
        };
        socket.Send(JsonConvert.SerializeObject(data));
    }

    private void VerifyJwt(IWebSocketConnection socket)
    {
        throw new NotImplementedException();
    }

    public Task AddSocketToRoomConnections(IWebSocketConnection socket)
    {
        
        if (!socketConnections.ContainsKey(room))
            socketConnections[room] = new List<IWebSocketConnection>();
        socketConnections[room].Add(socket);
        Console.WriteLine("A client has entered the room!");
        //return BroadCastToRoom(room, "A client has entered the room!");
        return Task.CompletedTask;
    }

    private Task RemoveClientFromRooms(IWebSocketConnection webSocket)
    {
        // todo refactor til at fjerne fra alle rooms
        if (!socketConnections.ContainsKey(room))
            return null;
        socketConnections[room].Remove(webSocket);
        Console.WriteLine("A client has left the room!");
        //return BroadCastToRoom(room, "A client has left the room!");
        return Task.CompletedTask;
    }

    private void AddMessage(IWebSocketConnection socket, Message message)
    {
        //var jwt = socket.ConnectionInfo.Headers["Authorization"];
        //Console.WriteLine("jwt: "+jwt);

        Message messageToInsert = new Message()
        {
            messageContent = message.messageContent,
            room = message.room,
            sender = message.sender,
            timestamp = DateTimeOffset.UtcNow
        };
        var insertionResponse = new List<Message> { chatRepository.InsertMessage(messageToInsert!) };

        foreach (var s in socketConnections[socket.ConnectionInfo.Path])
        {
            s.Send(JsonConvert.SerializeObject(insertionResponse));
        }
    }
}

