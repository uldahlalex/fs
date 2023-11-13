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
            socket.OnOpen = () =>
            {
                AddToRoom(socket, socket.ConnectionInfo.Path);
                socket.Send(JsonConvert.SerializeObject(chatRepository.GetPastMessages()));
               
            };
            socket.OnClose = () => { RemoveClientFromRoom(socket, socket.ConnectionInfo.Path); };
            socket.OnMessage = message => AddMessage(socket.ConnectionInfo.Path, message);
        });
    }

    private void AddMessage(string roomToBroadCastTo, string message)
    {

        Message messageToInsert = new Message()
        {
            messageContent = message,
            room = int.Parse(roomToBroadCastTo.Split("/")[1]),
            sender = 1,
            timestamp = DateTimeOffset.UtcNow
        };
        var insertionResponse = new List<Message> { chatRepository.InsertMessage(messageToInsert!) };
       
        foreach (var socket in socketConnections[roomToBroadCastTo])
        {
            socket.Send(JsonConvert.SerializeObject(insertionResponse));
        }
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
}