using System.Security.Authentication;
using core.ExtensionMethods;
using core.Models;
using core.Models.WebsocketTransferObjects;
using core.SecurityUtilities;
using core.TextTools;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;
using Serilog;

namespace api.Websocket;

public class WebsocketServer(
    WebsocketLiveConnections websocketLiveConnections,
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
                    ClientWantsToAuthenticate(socket,
                        Deserializer<ClientWantsToAuthenticate>.Deserialize(incomingClientMessagePayload));
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

    private void ClientWantsToSendMessageToRoom(IWebSocketConnection socket,
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


    private void ClientWantsToEnterRoom(IWebSocketConnection socket, ClientWantsToEnterRoom clientWantsToEnterRoom)
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

    private void ClientWantsToLeaveRoom(IWebSocketConnection socket, ClientWantsToLeaveRoom clientWantsToLeaveRoom)
    {
        socket.RemoveFromRoom(clientWantsToLeaveRoom.roomId);
        websocketUtilities.BroadcastMessageToRoom(clientWantsToLeaveRoom.roomId, new ServerNotifiesClientsInRoom
        {
            message = "A user has left the room!",
            roomId = clientWantsToLeaveRoom.roomId
        });
        
    }

    private void ClientWantsToRegister(IWebSocketConnection socket, ClientWantsToRegister clientWantsToRegister)
    {
        if (chatRepository.UserExists(clientWantsToRegister.email)) throw new Exception("User already exists!");
        var salt = SecurityUtilities.GenerateSalt();
        var hash = SecurityUtilities.Hash(clientWantsToRegister.password!, salt);
        EndUser user = chatRepository.InsertUser(clientWantsToRegister.email!, hash, salt);
        var jwt = SecurityUtilities.IssueJwt(
            new Dictionary<string, object?>() { {"email", user.email} });
        socket.Authenticate();
        socket.Send(JsonConvert.SerializeObject(new ServerHasAuthenticatedUser{jwt = jwt}));
    }

    private void ClientWantsToAuthenticate(IWebSocketConnection socket, ClientWantsToAuthenticate clientWantsToAuthenticate)
    {
        EndUser user;
        try
        {
            user = chatRepository.GetUser(clientWantsToAuthenticate.email!);
        } catch (Exception e)
        {
            Log.Error(e, "WebsocketServer");
            throw new AuthenticationException("User does not exist!");
        }
        
        var expectedHash = SecurityUtilities.Hash(clientWantsToAuthenticate.password!, user.salt!);
        if(!expectedHash.Equals(user.hash)) throw new AuthenticationException("Wrong password!");
        var jwt = SecurityUtilities.IssueJwt(new Dictionary<string, object?>() { {"email", user.email} });
        socket.Authenticate();
        socket.Send(JsonConvert.SerializeObject(new ServerHasAuthenticatedUser{jwt = jwt}));
    }
}