using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Infrastructure;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace api;

public class WebsocketService(ChatRepository repository)
{
    private readonly ConcurrentDictionary<string, List<WebSocket>> _rooms = new();

    public async Task EstablishConnection(WebSocket webSocket, string room)
    {
        try
        {
            await AddToRoom(webSocket, room);
            var dataToSendToNewClient = repository.GetPastMessages();
            await SendDataToClient(webSocket, dataToSendToNewClient);
            while (webSocket.State == WebSocketState.Open)
            {
                await KeepSendingNewMessagesToOpenConnection(webSocket, room, new byte[4096]);
                await Task.Delay(1000);
            }

            await RemoveClientFromRoom(webSocket, room);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            await RemoveClientFromRoom(webSocket, room);
        }
    }


    private async Task KeepSendingNewMessagesToOpenConnection(WebSocket webSocket, string room,
        byte[] websocketPayloadBytes)
    {
        WebSocketReceiveResult webSocketReceiveResult =
            await webSocket.ReceiveAsync(new ArraySegment<byte>(websocketPayloadBytes), CancellationToken.None);
        if (webSocketReceiveResult.MessageType != WebSocketMessageType.Text)
        {
            await webSocket.CloseAsync(webSocketReceiveResult.CloseStatus.GetValueOrDefault(),
                webSocketReceiveResult.CloseStatusDescription,
                CancellationToken.None);
            return;
        }

        string rawMessageJsonString =
            Encoding.UTF8.GetString(websocketPayloadBytes, 0, webSocketReceiveResult.Count);
        Message messageToInsert = JsonSerializer.Deserialize<Message>(rawMessageJsonString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            }) ?? throw new InvalidOperationException("Could not deserialize into a Message object"); 
        var insertedMessage = repository.InsertMessage(messageToInsert!);
        await BroadCastToRoom(room, insertedMessage);
    }

    private async Task AddToRoom(WebSocket webSocket, string room)
    {
        if (!_rooms.ContainsKey(room))
            _rooms[room] = new List<WebSocket>();
        _rooms[room].Add(webSocket);
        await BroadCastToRoom(room, "A client has entered the room!");
    }

    private Task RemoveClientFromRoom(WebSocket webSocket, string room)
    {
        if (!_rooms.ContainsKey(room)) 
            return null;
        _rooms[room].Remove(webSocket);
        Console.WriteLine("A client has left the room!");
        return BroadCastToRoom(room, "A client has left the room!");
    }

    private Task BroadCastToRoom(string room, object message)
    {
        var tasks = _rooms[room].Select(async client =>
        {
            if (client.State == WebSocketState.Open)
            {
                await SendDataToClient(client, message);
            }
        });

        return Task.WhenAll(tasks);
    }

    private static Task SendDataToClient(WebSocket webSocket, object data)
    {
        // TODO faster serialization?
        var serialized = JsonConvert.SerializeObject(data);
        return webSocket.SendAsync(new ArraySegment<byte>(
                Encoding.UTF8.GetBytes(serialized)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }
}