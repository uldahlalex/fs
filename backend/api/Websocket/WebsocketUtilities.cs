using System.ComponentModel.DataAnnotations;
using core;
using Fleck;
using Newtonsoft.Json;
using System.Text.Json;
using core.ExtensionMethods;
using core.Models.WebsocketTransferObjects;
using Serilog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace api;

public class WebsocketUtilities(WebsocketLiveConnections websocketLiveConnections)
{
    public void BroadcastMessageToRoom(int roomId, BaseTransferObject transferObject)
    {
        foreach (var socketKeyValuePair in websocketLiveConnections.SocketState)
        {
            if (!socketKeyValuePair.Value.GetConnectedRooms().Contains(roomId))
                throw new KeyNotFoundException("User is not present in the room they are trying to send a message to!");
            try
            {
                var exp = websocketLiveConnections.SocketState.GetValueOrDefault(socketKeyValuePair.Key) ?? throw new Exception("Could not find socket with GUID "+socketKeyValuePair.Key);
                exp.Send(JsonConvert.SerializeObject(transferObject));
            }
            catch (Exception e)
            {
                Log.Error(e, "WebsocketUtilities");
            }
        }
    }

    public void PurgeClient(IWebSocketConnection socket)
    {
        if (websocketLiveConnections.SocketState.ContainsKey(socket.ConnectionInfo.Id))
            websocketLiveConnections.SocketState.Remove(socket.ConnectionInfo.Id, out _);
    }

    public void EventNotFound(IWebSocketConnection socket, string eventType)
    {
        var response = new ServerSendsErrorMessageToClient()
        {
            errorMessage = "Unknown event!",
            receivedEventType = eventType
        };
        socket.Send(JsonConvert.SerializeObject(response));
    }
}

