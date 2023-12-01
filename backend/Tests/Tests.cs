using api.Websocket;
using core;
using core.ExtensionMethods;
using core.Models.WebsocketTransferObjects;
using FluentAssertions;
using Infrastructure;
using Npgsql;
using NUnit.Framework;
using Serilog;
using Websocket.Client;

namespace Tests;

public class WebsocketServerTests
{
    private readonly WebsocketServer _webSocketServer;
    private Uri _uri = new Uri("ws://localhost:8181");

    public WebsocketServerTests()
    {
        
       /*Log.Logger = new LoggerConfiguration() // todo put in shared
            .WriteTo.Console(outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
            .CreateLogger();
        _webSocketServer = new WebsocketServer(
            new ChatRepository(
                new NpgsqlDataSourceBuilder(
                    Utilities.ProperlyFormattedConnectionString).Build()));
        _webSocketServer.StartWebsocketServer();*/
    }

    [TestCase(1)]
    public async Task ServerBroadcastsSomeDataToSeveralClients(int roomId)
    {
        var messagesSentToClient1 = new List<ServerNotifiesClientsInRoom>();
        var messagesSentToClient2 = new List<ServerNotifiesClientsInRoom>();
        var enterRoomEvent = new ClientWantsToEnterRoom
        {
            roomId = roomId
        };
        var expectedNotification = new ServerNotifiesClientsInRoom()
        {
            roomId = roomId,
            message = "A new user has entered the room!"
            //todo add user to the notification?
        };
        using (var client = new WebsocketClient(_uri))
        using (var client2 = new WebsocketClient(_uri))
        {
            client.MessageReceived.Subscribe(message =>
            {
                Log.Information("CLIENT 1: GETS: "+message.Text);
                if (message.Text.Deserialize<BaseTransferObject>().eventType == nameof(ServerNotifiesClientsInRoom))
                    messagesSentToClient1.Add(message.Text.Deserialize<ServerNotifiesClientsInRoom>());
            });

            client2.MessageReceived.Subscribe(message =>
            {
                Log.Information("CLIENT 2: GETS: "+message.Text);
                if (message.Text.Deserialize<BaseTransferObject>().eventType == nameof(ServerNotifiesClientsInRoom))
                    messagesSentToClient2.Add(message.Text.Deserialize<ServerNotifiesClientsInRoom>());

            });

            await client.Start();
            await client2.Start();
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            //the bug has been revealed: exc it thrown by server when
            //a socket conn is open but another broadcasts to a room the first one is not in
            //here the behvaior should be: one client shoud ge the message, because only one client is present in the room
            client.Send(enterRoomEvent.ToJsonString());
            client2.Send(enterRoomEvent.ToJsonString());
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

       messagesSentToClient1
            .Should()
            .ContainEquivalentOf(expectedNotification);
        messagesSentToClient2
            .Should()
            .ContainEquivalentOf(expectedNotification);
    }
}