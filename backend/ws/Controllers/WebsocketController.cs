using System.Collections.Concurrent;
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
        byte[] tempMessage = new byte[1024 * 4]; // Message reader
        
        while (connectionAlive)
        {

            webSocketPayload.Clear();

            WebSocketReceiveResult? webSocketResponse;

            // Read message in a loop until fully read
            do
            {
                // Wait until client sends message
                webSocketResponse = await socket.ReceiveAsync(tempMessage, CancellationToken.None);

                // Save bytes
                webSocketPayload.AddRange(new ArraySegment<byte>(tempMessage, 0, webSocketResponse.Count));
            } while (webSocketResponse.EndOfMessage == false);

            // Process the message
            if (webSocketResponse.MessageType == WebSocketMessageType.Text)
            {
                // 3. Convert textual message from bytes to string
                string message = System.Text.Encoding.UTF8.GetString(webSocketPayload.ToArray());
                Websockets.Add(socket);
                Console.WriteLine("Client says {0}", message);
                string receivedMessage = await ReadFromWebSocket(socket);
                var bf = new ArraySegment<byte>(Encoding.UTF8.GetBytes(receivedMessage + " RESPONSE"));
                foreach (var client in Websockets)
                {
                    await client.SendAsync(bf, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            else if (webSocketResponse.MessageType == WebSocketMessageType.Close)
            {
                connectionAlive = false;
            }
        }
        Console.WriteLine(" -> A client disconnected.");
    }

    private static async Task<string> ReadFromWebSocket(System.Net.WebSockets.WebSocket webSocket)
    {
        var buffer = new ArraySegment<byte>(new byte[1024 * 4]);

        using (var ms = new MemoryStream())
        {
            WebSocketReceiveResult result;
            do
            {
                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                ms.Write(buffer.Array, buffer.Offset, result.Count);
            } while (!result.EndOfMessage);

            ms.Seek(0, SeekOrigin.Begin);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                using (var reader = new StreamReader(ms, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        return null;
    }
}


