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
        await HandleMessages(webSocket, room);
        await HandleDisconnection(webSocket, room);
    }

    private async Task HandleMessages(WebSocket webSocket, string room)
    {
        byte[] bytes = new byte[4096];
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                // Listen for new messages
                WebSocketReceiveResult result =
                    await webSocket.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None);

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
                            await WebSocketUtilities.SendMessageToWebsocketConnectionAsync(client, insertedMessage!);
                        }
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    // If the client has sent a close frame
                    await webSocket.CloseAsync(result.CloseStatus.GetValueOrDefault(), result.CloseStatusDescription,
                        CancellationToken.None);
                }
            }
        }
        catch (WebSocketException)
        {
            // Handle case when socket is abruptly closed
            Console.WriteLine("A websocket exception has occured!");
        }
    }

    private async Task SendMessageToAllClientsInRoom(Message insertedMessage, string room)
    {
        foreach (var client in Rooms[room])
        {
            if (client.State == WebSocketState.Open)
            {
                await WebSocketUtilities.SendMessageToWebsocketConnectionAsync(client, insertedMessage!);
            }
        }
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