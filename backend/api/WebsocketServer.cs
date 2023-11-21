using core;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;
using Serilog;

namespace api;

public class WebsocketServer(
    State state,
    Events events,
    WebsocketUtilities websocketUtilities,
    AuthUtilities auth,
    ChatRepository chatRepository)
{
    //excceptional
    //extensoin method for error handling
    //delegates
    //PostSharp

    //ext method refactor


    public void Start()
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
                .Deserialize(incomingClientMessagePayload, socket)
                .eventType;
            switch (eventType)
            {
                case "ClientWantsToEnterRoom":
                    events.ClientWantsToEnterRoom(socket,
                        Deserializer<ClientWantsToEnterRoom>.Deserialize(incomingClientMessagePayload, socket));
                    break;
                case "ClientWantsToSendMessageToRoom":
                    events.ClientWantsToSendMessageToRoom(socket,
                        Deserializer<ClientSendsMessageToRoom>.Deserialize(incomingClientMessagePayload, socket));
                    break;
                case "ClientWantsToLeaveRoom":
                    events.ClientWantsToLeaveRoom(socket,
                        Deserializer<ClientWantsToLeaveRoom>.Deserialize(incomingClientMessagePayload, socket));
                    break;
                default: //fungerer dette default?
                    websocketUtilities.EventNotFound(socket);
                    break;
            } //todo data validation and eventType not found
        }
        catch (DeserializationException e)
        {
            Log.Error(e, "WebsocketServer");
            var response = JsonConvert.SerializeObject(new ServerSendsErrorMessageToClient
            {
                errorMessage = e.Message
            });
            socket.Send(response);
        }
        catch (Exception e)
        {
            Log.Error(e, "WebsocketServer");
            var response = JsonConvert.SerializeObject(new ServerSendsErrorMessageToClient
            {
                errorMessage = "Internal server error"
            });
            socket.Send(response);
        }
    }
}