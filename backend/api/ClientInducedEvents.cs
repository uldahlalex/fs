using core;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;
using Serilog;

namespace api;

public class ClientInducedEvents(
    ChatRepository chatRepository,
    State state,
    WebsocketUtilities websocketUtilities,
    AuthUtilities authUtilities)
{
    
    public void ClientWantsToSendMessageToRoom(IWebSocketConnection socket,
        ClientSendsMessageToRoom clientSendsMessageToRoom)
    {
        Message messageToInsert = new Message()
        {
            messageContent = clientSendsMessageToRoom.messageContent,
            room = clientSendsMessageToRoom.roomId,
            sender = 1, //todo refactor til at tage fra jwt
            timestamp = DateTimeOffset.UtcNow
        };
        var insertionResponse = chatRepository.InsertMessage(messageToInsert);
        var response = new ServerBroadcastsMessageToClients()
        {
            message = insertionResponse
        };

        websocketUtilities.BroadcastMessageToRoom(clientSendsMessageToRoom.roomId, response);
    }


    public void ClientWantsToEnterRoom(IWebSocketConnection socket, ClientWantsToEnterRoom clientWantsToEnterRoom)
    {
        if (!state.AllSockets.ContainsKey(socket.ConnectionInfo.Id))
            return;
        socket.JoinRoom(clientWantsToEnterRoom.roomId);
        var data = new ServerLetsClientEnterRoom()
        {
            recentMessages = chatRepository.GetPastMessages(),
            roomId = clientWantsToEnterRoom.roomId,
        };
        socket.Send(JsonConvert.SerializeObject(data));
        websocketUtilities.BroadcastMessageToRoom(clientWantsToEnterRoom.roomId, new ServerNotifiesClientsInRoom()
        {
            roomId = clientWantsToEnterRoom.roomId,
            message = "A new user has entered the room!"
        });
    }
    
    public void ClientWantsToLeaveRoom(IWebSocketConnection socket, ClientWantsToLeaveRoom clientWantsToLeaveRoom)
    {
        socket.RemoveFromRoom(clientWantsToLeaveRoom.roomId);
        websocketUtilities.BroadcastMessageToRoom(clientWantsToLeaveRoom.roomId, new ServerNotifiesClientsInRoom
        {
            message = "A user has left the room!",
            roomId = clientWantsToLeaveRoom.roomId
        });
        
    }
    
    public void ClientWantsToRegister(IWebSocketConnection socket, ClientWantsToRegister clientWantsToRegister)
    {
        var jwt = authUtilities.IssueJwt("1234", new Dictionary<string, object>()
        {
            {"email", clientWantsToRegister.email}
        });
        Log.Information(jwt);
        var result = authUtilities.IsJwtValid(jwt, "1234");
        Log.Information(result.ToString());
        socket.Authenticate();
        socket.Send(jwt);
    }
}