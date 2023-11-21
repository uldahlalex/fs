using core;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;

namespace api;

public class Events(ChatRepository chatRepository, State state, WebsocketUtilities websocketUtilities)
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
        if (!state._allSockets.ContainsKey(socket.ConnectionInfo.Id))
            return; //todo der skal sikkert throwes exc
        socket.JoinRoom(clientWantsToEnterRoom.roomId);
        var data = new ServerLetsClientEnterRoom()
        {
            recentMessages = chatRepository.GetPastMessages(),
            roomId = clientWantsToEnterRoom.roomId,
        };
        socket.Send(JsonConvert.SerializeObject(data));
        //send to all other in room
    }
    
    public void ClientWantsToLeaveRoom(IWebSocketConnection socket, ClientWantsToLeaveRoom clientWantsToLeaveRoom)
    {
        socket.RemoveFromRoom(clientWantsToLeaveRoom.roomId);
        //notify people in room
    }
}