using System.Collections.Concurrent;
using System.Linq.Expressions;
using core;
using Fleck;
using Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using Serilog;
using Serilog.Core;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace api;

public class FleckServer(
    State state,
    Events events,
    ILogger<FleckServer> logger,
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
            List<int> a = [];
            var s = a[1];
            var server = new WebSocketServer("ws://127.0.0.1:8181");
            server.RestartAfterListenError = true;
            server.Start(socket =>
            {
                //socket.IsAuthenticated();
                socket.OnMessage = message => HandleClientMessage(socket, message);
                socket.OnOpen = () => { state._allSockets.TryAdd(socket.ConnectionInfo.Id, socket); };
                socket.OnClose = () => { websocketUtilities.PurgeClient(socket); };
                socket.OnError = exception =>
                {
                    //probably doesnt replace catch blocks
                    logger.Log(LogLevel.Critical, exception, "FleckServer");
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
            Log.Information(e, "FleckServer");
            Log.Error(e, "FleckServer");
            Log.Fatal(e, "FleckServer");
            Log.Warning(e, "FleckServer");
            Log.Debug(e, "FleckServer");
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
                    ExceptionHandlers.TryExecute(() => events.ClientWantsToEnterRoom(socket,
                            Deserializer<ClientWantsToEnterRoom>.Deserialize(incomingClientMessagePayload, socket)),
                        exception => { });
                    break;
                case "ClientWantsToSendMessageToRoom":
                    events.ClientWantsToSendMessageToRoom(socket,
                        Deserializer<ClientSendsMessageToRoom>.Deserialize(incomingClientMessagePayload, socket));
                    break;
                default:
                    throw new Exception("not found"); //er det altid ønskværdigt med resp?
            } //todo data validation and eventType not found
        }
        catch (DeserializationException e)
        {
            
        }
    }
}

public class DeserializationException : Exception
{
    public DeserializationException()
    {
    }

    public DeserializationException(string message)
        : base(message)
    {
    }

    public DeserializationException(string message, Exception inner)
        : base(message, inner)
    {
    }
}

public class DeserializationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool>
        TryHandleAsync(HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
    {
        return true;
    }
}