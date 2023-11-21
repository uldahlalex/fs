using core;
using Fleck;
using Newtonsoft.Json;
using Serilog;

namespace api;

public class WebsocketServer(
    State state,
    Events events,
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
                socket.OnOpen = () => { state.AllSockets.TryAdd(socket.ConnectionInfo.Id, socket); };
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
                    events.ClientWantsToEnterRoom(socket,
                        Deserializer<ClientWantsToEnterRoom>.DeserializeAndValidate(incomingClientMessagePayload));
                    break;
                case "ClientWantsToSendMessageToRoom":
                    events.ClientWantsToSendMessageToRoom(socket,
                        Deserializer<ClientSendsMessageToRoom>.Deserialize(incomingClientMessagePayload));
                    break;
                case "ClientWantsToLeaveRoom":
                    events.ClientWantsToLeaveRoom(socket,
                        Deserializer<ClientWantsToLeaveRoom>.Deserialize(incomingClientMessagePayload));
                    break;
                case "ClientWantsToRegister":
                    //todo
                    break;
                case "ClientWantsToAuthenticate":
                    //todo
                    break;
                default: 
                    websocketUtilities.EventNotFound(socket);
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
}