using core;
using Fleck;
using Newtonsoft.Json;
using System.Text.Json;
using Serilog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace api;

public class WebsocketUtilities(State state)
{
    public void BroadcastMessageToRoom(int roomId, BaseTransferObject transferObject)
    {
        foreach (var socketGuid in state._allSockets)
        {
            if (socketGuid.Value.ConnectedRooms().Contains(roomId))
            {
                try
                {
                    var exp = state._allSockets.GetValueOrDefault(socketGuid.Key) ?? throw new Exception("Could not find socket with GUID "+socketGuid.Key);
                    exp.Send(JsonConvert.SerializeObject(transferObject));
                }
                catch (Exception e)
                {
                    Log.Error(e, "WebsocketUtilities");
                }
        
            }
        }
    }

    public void PurgeClient(IWebSocketConnection socket)
    {
        if (state._allSockets.ContainsKey(socket.ConnectionInfo.Id))
            state._allSockets.Remove(socket.ConnectionInfo.Id, out _);
    }
}

public static class Deserializer<T>
{
    public static T Deserialize(string message, IWebSocketConnection socket)
    {
        return JsonSerializer.Deserialize<T>(message,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new DeserializationException($"Failed to deserialize message: {message}");
    }
}