using core;
using core.AuthenticationUtilities;
using core.ExtensionMethods;
using core.Models;
using core.Models.WebsocketTransferObjects;
using core.TextTools;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;
using Serilog;

namespace api;

public class WebsocketServer(
    WebsocketLiveConnections websocketLiveConnections,
    AuthUtilities authUtilities,
    ChatRepository chatRepository,
    WebsocketUtilities websocketUtilities)
{
    public void StartWebsocketServer()
    {
        try
        {
            var server = new WebSocketServer("ws://127.0.0.1:8181");
            server.RestartAfterListenError = true;
            server.Start(socket =>
            {
                socket.OnMessage = message => HandleClientMessage(socket, message);
                socket.OnOpen = () => { websocketLiveConnections.SocketState.TryAdd(socket.ConnectionInfo.Id, socket); };
                socket.OnClose = () => { websocketUtilities.PurgeClient(socket); };
                socket.OnError = exception =>
                {
                    Log.Error(exception, "WebsocketServer");
                    var data = new ServerSendsErrorMessageToClient()
                    {
                        errorMessage = exception.Message
                    };
                    socket.Send(JsonConvert.SerializeObject(data));
                };
            });
        }
        catch (Exception e)
        {
            Log.Error(e, "WebsocketServer");
        }
    }


    private void HandleClientMessage(IWebSocketConnection socket, string incomingClientMessagePayload)
    {
        try
        {
            string eventType = Deserializer<BaseTransferObject>
                .Deserialize(incomingClientMessagePayload)
                .eventType;
            switch (eventType)
            {
                case "ClientWantsToEnterRoom":
                    ClientWantsToEnterRoom(socket,
                        Deserializer<ClientWantsToEnterRoom>.DeserializeAndValidate(incomingClientMessagePayload));
                    break;
                case "ClientWantsToSendMessageToRoom":
                    ClientWantsToSendMessageToRoom(socket,
                        Deserializer<ClientSendsMessageToRoom>.Deserialize(incomingClientMessagePayload));
                    break;
                case "ClientWantsToLeaveRoom":
                    ClientWantsToLeaveRoom(socket,
                        Deserializer<ClientWantsToLeaveRoom>.Deserialize(incomingClientMessagePayload));
                    break;
                case "ClientWantsToRegister":
                    ClientWantsToRegister(socket,
                        Deserializer<ClientWantsToRegister>.Deserialize(incomingClientMessagePayload));
                    break;
                case "ClientWantsToAuthenticate":
                    //todo
                    break;
                default: 
                    websocketUtilities.EventNotFound(socket, eventType);
                    break;
            } 
        }
        catch (Exception e)
        {
            Log.Error(e, "WebsocketServer");
            var response = JsonConvert.SerializeObject(new ServerSendsErrorMessageToClient
            {
                errorMessage = e.Message
            });
            socket.Send(response);
        }
    }
    
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
        if (!websocketLiveConnections.SocketState.ContainsKey(socket.ConnectionInfo.Id))
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
        var jwt = authUtilities.IssueJwt("1234", new Dictionary<string, object?>()
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