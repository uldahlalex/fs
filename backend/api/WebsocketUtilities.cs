using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace api.Controllers.Utility
{
    public static class WebSocketUtilities
    {
        public static async Task SendMessageToWebsocketConnectionAsync(WebSocket webSocket, object data)
        {
            var serialized = JsonConvert.SerializeObject(data);
            await webSocket.SendAsync(new ArraySegment<byte>(
                    Encoding.UTF8.GetBytes(serialized)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        public static async Task<string> ReceiveMessage(WebSocket webSocket)
        {
            byte[] bytes = new byte[4 * 1024];
            var response = await webSocket.ReceiveAsync(new ArraySegment<byte>(bytes), CancellationToken.None);
            return Encoding.UTF8.GetString(bytes, 0, response.Count);
            
        }

        public static async Task CloseSocket(WebSocket webSocket, WebSocketCloseStatus status, string statusDescription)
        {
            await webSocket.CloseAsync(status, statusDescription, CancellationToken.None);
        }
    }
}