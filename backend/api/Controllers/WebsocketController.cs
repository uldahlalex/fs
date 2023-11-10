using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers;
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

            if (!Rooms.ContainsKey(room))
            {
                Rooms[room] = new List<WebSocket>();
            }

            Rooms[room].Add(webSocket);
            await EstablishConnection(webSocket, room);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }

    private async Task<string> GetMessageFromWebsocketAsync(WebSocket webSocket)
    {
        var bytes = new byte[4 * 1024];
        return Encoding.UTF8.GetString(bytes, 0,
            (await webSocket.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None)).Count);
    }

    private Task SendMessageAsync(WebSocket webSocket, string message)
    {
        return webSocket.SendAsync(new ArraySegment<byte>(
                Encoding.UTF8.GetBytes(message)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    private async Task EstablishConnection(WebSocket webSocket, string room)
    {
        Console.WriteLine($" -> A new client connected to room: {room}");

        await SendMessageAsync(webSocket, repository.GetPastMessages().ToList()[0].MessageContent);
        
        while (webSocket.State == WebSocketState.Open)
        {
            var message = await GetMessageFromWebsocketAsync(webSocket);
            foreach (var client in Rooms[room])
            {
                if (client.State == WebSocketState.Open)
                {
                    await SendMessageAsync(client, message);
                }
            }
        }

        var msg = $" -> A client disconnected from room: {room}";
        Console.WriteLine(msg);
        Rooms[room].Remove(webSocket);
        foreach (var client in Rooms[room])
        {
            if (client.State == WebSocketState.Open)
            {
                await SendMessageAsync(client, msg);
            }
        }
    }
}