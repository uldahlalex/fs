using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace ws.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketController : ControllerBase
    {
        private static readonly List<WebSocket> websockets = new List<WebSocket>();

        [Route("/ws")]
        public async Task Get()
        {
            var context = HttpContext;
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine(" -> A new client connected!");

                websockets.Add(webSocket);
                
                await EchoLoop(webSocket);
            }
            else
            {
                context.Response.StatusCode = 400;
            }
        }

        private async Task EchoLoop(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024 * 4]; //4KB buffer size
            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                foreach (var client in websockets)
                {
                    if(client.State == WebSocketState.Open)
                    {
                        await client.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    }
                }
            }
            websockets.Remove(webSocket);
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }
    }
}