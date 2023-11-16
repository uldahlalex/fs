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
                //skal room refactores?
                //check jwt når conn åbnes
                //AddToRoom(socket, socket.ConnectionInfo.Path); //add to room er kun for live connection - ikke persist
                //socket.Send(JsonConvert.SerializeObject(chatRepository.GetPastMessages())); //skal man virkelig have alle messages for alle rooms når man logger ind?
            };
            socket.OnClose = () => { RemoveClientFromRoom(socket, socket.ConnectionInfo.Path); };
        });
    }

    private void GetIncomingMessage(IWebSocketConnection socket, string message)
    {
        TransferObject deserialized = JsonConvert.DeserializeObject<TransferObject>(message);
        if (deserialized.Action == "addMessage")
            AddMessage(socket, deserialized.Data.ToObject<Message>());
        //create room
    }


    public Task AddToRoom(IWebSocketConnection socket, string room)
    {
        if (!socketConnections.ContainsKey(room))
            socketConnections[room] = new List<IWebSocketConnection>();
        socketConnections[room].Add(socket);
        Console.WriteLine("A client has entered the room!");
        //return BroadCastToRoom(room, "A client has entered the room!");
        return Task.CompletedTask;
    }

    private Task RemoveClientFromRoom(IWebSocketConnection webSocket, string room)
    {
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