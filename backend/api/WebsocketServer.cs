using core;
using Fleck;
using Newtonsoft.Json;
using Serilog;

namespace api;

public class WebsocketServer(
    State state,
    ClientInducedEvents clientInducedEvents,
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
                    clientInducedEvents.ClientWantsToEnterRoom(socket,
                        Deserializer<ClientWantsToEnterRoom>.DeserializeAndValidate(incomingClientMessagePayload));
                    break;
                case "ClientWantsToSendMessageToRoom":
                    clientInducedEvents.ClientWantsToSendMessageToRoom(socket,
                        Deserializer<ClientSendsMessageToRoom>.Deserialize(incomingClientMessagePayload));
                    break;
                case "ClientWantsToLeaveRoom":
                    clientInducedEvents.ClientWantsToLeaveRoom(socket,
                        Deserializer<ClientWantsToLeaveRoom>.Deserialize(incomingClientMessagePayload));
                    break;
                case "ClientWantsToRegister":
                    clientInducedEvents.ClientWantsToRegister(socket,
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
}