using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

[ApiController]
[Route("[controller]")]
public class WebSocketController(ChatRepository repository) : ControllerBase
{
    private static readonly ConcurrentDictionary<string, List<WebSocket>> Rooms = new();

    [Route("/api/{room}")]
    public async Task Get(string room)
    {
        var context = HttpContext;
        if (context.WebSockets.IsWebSocketRequest)
        {
            WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await EstablishConnection(webSocket, room);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }


    private async Task EstablishConnection(WebSocket webSocket, string room)
    {
        await AddToRoom(webSocket, room);
        var dataToSendToNewClient = repository.GetPastMessages();
        await SendDataToClient(webSocket, dataToSendToNewClient);
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                await SendNewMessagesToConnection(webSocket, room, new byte[4096]);
            }
        }
        catch (WebSocketException)
        {
            Console.WriteLine("A websocket exception has occured!");
            throw;
        }

        await RemoveClientFromRoom(webSocket, room);
    }

    private async Task SendNewMessagesToConnection(WebSocket webSocket, string room, byte[] websocketPayloadBytes)
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

        var messageToInsert = JsonConvert.DeserializeObject<Message>
            (Encoding.UTF8.GetString(websocketPayloadBytes, 0, webSocketReceiveResult.Count));
        var insertedMessage = repository.InsertMessage(messageToInsert!);
        await BroadCastToRoom(room, insertedMessage);
    }
    public async Task AddToRoom(WebSocket webSocket, string room)
    {
        if (!Rooms.ContainsKey(room))
            Rooms[room] = new List<WebSocket>();
        Rooms[room].Add(webSocket);
        await BroadCastToRoom(room, "A client has entered the room!");
    }

    public async Task RemoveClientFromRoom(WebSocket webSocket, string room)
    {
        Rooms[room].Remove(webSocket);
        Console.WriteLine("A client has left the room!");
        await BroadCastToRoom(room, "A client has left the room!");
    }

    private async Task BroadCastToRoom(string room, object message)
    {
        foreach (var client in Rooms[room])
        {
            if (client.State == WebSocketState.Open)
            {
                await SendDataToClient(client, message);
            }
        }
    }

    private static Task SendDataToClient(WebSocket webSocket, object data)
    {
        var serialized = JsonConvert.SerializeObject(data);
        return webSocket.SendAsync(new ArraySegment<byte>(
                Encoding.UTF8.GetBytes(serialized)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }
}