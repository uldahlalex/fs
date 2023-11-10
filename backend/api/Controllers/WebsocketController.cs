using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

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

    private Task SendMessageAsync(WebSocket webSocket, object data)
    {
        var serialized = JsonConvert.SerializeObject(data);
        return webSocket.SendAsync(new ArraySegment<byte>(
                Encoding.UTF8.GetBytes(serialized)),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

   private async Task EstablishConnection(WebSocket webSocket, string room)
{
    Console.WriteLine($" -> A new client connected to room: {room}");

    await SendMessageAsync(webSocket, repository.GetPastMessages());

    // Create a byte array for the receive buffer
    byte[] bytes = new byte[4096];

    // Keep listening as long as the WebSocket is open
    try
    {
        while (webSocket.State == WebSocketState.Open)
        {
            // Listen for new messages
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None);

            // Handle the received message based on its MessageType
            if (result.MessageType == WebSocketMessageType.Text)
            {
                // Extract message from the buffer and process it
                var message = Encoding.UTF8.GetString(bytes, 0, result.Count);
                var obj = JsonConvert.DeserializeObject<Message>(message);
                var insertedMessage = repository.InsertMessage(obj);

                // Send the message to all clients in the room
                foreach (var client in Rooms[room])
                {
                    if (client.State == WebSocketState.Open)
                    {
                        await SendMessageAsync(client, insertedMessage!);
                    }
                }
            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                // If the client has sent a close frame
                await webSocket.CloseAsync(result.CloseStatus.GetValueOrDefault(), result.CloseStatusDescription, CancellationToken.None);
            }
        }
    }
    catch (WebSocketException)
    {
        // Handle case when socket is abruptly closed
    }

    var msg = $" -> A client disconnected from room: {room}";
    Console.WriteLine(msg);
    Rooms[room].Remove(webSocket);

    // Notify other clients in the room about the disconnection
    foreach (var client in Rooms[room])
    {
        if (client.State == WebSocketState.Open)
        {
            await SendMessageAsync(client, msg);
        }
    }
}
}