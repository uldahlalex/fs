using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace ws.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketController : ControllerBase
    {
        private static readonly ConcurrentDictionary<string, List<WebSocket>> rooms = new ConcurrentDictionary<string, List<WebSocket>>();

        [Route("/ws/{room}")]
        public async Task Get(string room)
        {
            var context = HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                
                if (!rooms.ContainsKey(room))
                {
                    rooms[room] = new List<WebSocket>();
                }
                rooms[room].Add(webSocket);

                Console.WriteLine($" -> A new client connected to room: {room}");

                await EchoLoop(webSocket, room);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        private async Task EchoLoop(WebSocket webSocket, string room)
        {
            var buffer = new byte[1024 * 4];
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                foreach (var client in rooms[room])
                {
                    if (client.State == WebSocketState.Open && result.MessageType != WebSocketMessageType.Close)
                    {
                        await client.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                }
            }
            Console.WriteLine($" -> A client disconnected from room: {room}");
            // Construct a disconnect message
            string disconnectMessage = "A client has disconnected!";
            var disconnectMessageBuffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(disconnectMessage));
            rooms[room].Remove(webSocket);
            foreach (var client in rooms[room])
            {
                if (client.State == WebSocketState.Open)
                {
                    await client.SendAsync(disconnectMessageBuffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }  
        }
    }
}