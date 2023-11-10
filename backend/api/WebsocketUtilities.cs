using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace api.Controllers.Utility
{
    public static class WebSocketUtilities
    {
        public static Task SendMessageToWebsocketConnectionAsync(WebSocket webSocket, object data)
        {
            var serialized = JsonConvert.SerializeObject(data);
            return webSocket.SendAsync(new ArraySegment<byte>(
                    Encoding.UTF8.GetBytes(serialized)),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }


    }
}