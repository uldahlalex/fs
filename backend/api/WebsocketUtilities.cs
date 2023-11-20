using core;
using Fleck;
using Newtonsoft.Json;

namespace api;

public class WebsocketUtilities(State state)
{
    public void BroadcastMessageToRoom(int roomId, BaseTransferObject transferObject)
    {
        foreach (var socketGuid in state._socketsConnectedToRoom[roomId])
        {
            var exp = state._allSockets.TryGetValue(socketGuid, out var socketToSendTo)
                ? socketToSendTo
                : throw new Exception("Socket not found");
            socketToSendTo.Send(JsonConvert.SerializeObject(transferObject));
        }
    } 

    public void PurgeClient(IWebSocketConnection socket)
    {
        foreach (var room in state._socketsConnectedToRoom)
        {
            if (room.Value.Contains(socket.ConnectionInfo.Id))
                room.Value.Remove(socket.ConnectionInfo.Id);
        }
        {
            if (state._allSockets.ContainsKey(socket.ConnectionInfo.Id))
                state._allSockets.Remove(socket.ConnectionInfo.Id, out _);
        }
    }
}