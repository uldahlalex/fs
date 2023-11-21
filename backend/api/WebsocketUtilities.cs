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
        foreach (var socketGuid in state.AllSockets)
        {
            if (socketGuid.Value.GetConnectedRooms().Contains(roomId))
            {
                try
                {
                    var exp = state.AllSockets.GetValueOrDefault(socketGuid.Key) ?? throw new Exception("Could not find socket with GUID "+socketGuid.Key);
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
        if (state.AllSockets.ContainsKey(socket.ConnectionInfo.Id))
            state.AllSockets.Remove(socket.ConnectionInfo.Id, out _);
    }

    public void EventNotFound(IWebSocketConnection socket)
    {
        var response = new ServerSendsErrorMessageToClient()
        {
            errorMessage = "Unknown event!"
        };
        socket.Send(JsonConvert.SerializeObject(response));
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