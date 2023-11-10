using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using api.Controllers.Utility;
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
        if (!Rooms.ContainsKey(room))
            Rooms[room] = new List<WebSocket>();
        Rooms[room].Add(webSocket);
        Console.WriteLine($" -> A new client connected to room: {room}");

        await WebSocketUtilities.SendMessageToWebsocketConnectionAsync(webSocket, repository.GetPastMessages());
        byte[] websocketPayloadBytes = new byte[4096];
        try
        {
            while (webSocket.State == WebSocketState.Open)
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
                // Extract message from the buffer and process it
                var message = Encoding.UTF8.GetString(websocketPayloadBytes, 0, webSocketReceiveResult.Count);
                var obj = JsonConvert.DeserializeObject<Message>(message);
                var insertedMessage = repository.InsertMessage(obj);

                foreach (var client in Rooms[room])
                {
                    if (client.State == WebSocketState.Open)
                    {
                        await WebSocketUtilities.SendMessageToWebsocketConnectionAsync(client, insertedMessage!);
                    }
                }
            }
        }
        catch (WebSocketException)
        {
            Console.WriteLine("A websocket exception has occured!");
            throw;
        }
        
        await HandleDisconnection(webSocket, room);
    }

    

    private async Task HandleDisconnection(WebSocket webSocket, string room)
    {
        var msg = $" -> A client disconnected from room: {room}";
        Console.WriteLine(msg);
        Rooms[room].Remove(webSocket);
        foreach (var client in Rooms[room])
        {
            if (client.State == WebSocketState.Open)
            {
                await WebSocketUtilities.SendMessageToWebsocketConnectionAsync(client, msg);
            }
        }
    }
}