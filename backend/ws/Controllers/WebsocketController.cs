using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace ws.Controllers;

[ApiController]
[Route("[controller]")]
public class WebSocketController() : ControllerBase
{
    private static readonly List<WebSocket> Websockets = new();

    [Route("/ws")]
    public async Task Get()
    {
        var context = HttpContext;
        using var socket = await context.WebSockets.AcceptWebSocketAsync();

        Console.WriteLine(" -> A new client connected!");

        bool connectionAlive = true;
        List<byte> webSocketPayload = new List<byte>(1024 * 4); // 4 KB initial capacity
        webSocketPayload.Clear();
        byte[] tempMessage = new byte[1024 * 4]; // Message reader

    
        WebSocketReceiveResult? webSocketResponse = await socket.ReceiveAsync(tempMessage, CancellationToken.None);

        webSocketPayload.AddRange(new ArraySegment<byte>(tempMessage, 0, webSocketResponse.Count));

        
        string message = Encoding.UTF8.GetString(webSocketPayload.ToArray());
        Websockets.Add(socket);
        Console.WriteLine("Client says {0}", message);
        var bf = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message + " RESPONSE"));
        foreach (var client in Websockets)
        {
            await client.SendAsync(bf, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}