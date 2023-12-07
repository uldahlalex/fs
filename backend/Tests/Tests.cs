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
    private readonly Uri _uri = new("ws://localhost:8181");

    public WebsocketServerTests()
    {
        Log.Logger = new LoggerConfiguration() //todo: does serilog also output to Rider test runner?
            .WriteTo.Console(
                outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
            .CreateLogger();
    }

    [SetUp]
    public void Setup()
    {
        new WebsocketServer(
                new ChatRepository(
                    new NpgsqlDataSourceBuilder(
                        Utilities.ProperlyFormattedConnectionString).Build()))
            .StartWebsocketServer();
        //if you want to run server manually for the tests, simply comment out the lines above, although not recommended
    }

    [TestCase(1)]
    public async Task ClientCanEnterRoomAndNotifyUsersInRoom(int roomId)
    {
        var messagesSentToClient1 = new List<ServerNotifiesClientsInRoom>();
        var messagesSentToClient2 = new List<ServerNotifiesClientsInRoom>();
        var enterRoomEvent = new ClientWantsToEnterRoom
        {
            roomId = roomId
        };
        var expectedNotification = new ServerNotifiesClientsInRoom
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
                Log.Information("CLIENT 1: GETS: " + message.Text);
                if (message.Text.Deserialize<BaseTransferObject>().eventType == nameof(ServerNotifiesClientsInRoom))
                    messagesSentToClient1.Add(message.Text.Deserialize<ServerNotifiesClientsInRoom>());
            });

            client2.MessageReceived.Subscribe(message =>
            {
                Log.Information("CLIENT 2: GETS: " + message.Text);
                if (message.Text.Deserialize<BaseTransferObject>().eventType == nameof(ServerNotifiesClientsInRoom))
                    messagesSentToClient2.Add(message.Text.Deserialize<ServerNotifiesClientsInRoom>());
            });

            await client.Start();
            await client2.Start();
            await Task.Delay(TimeSpan.FromSeconds(1));

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

    [TestCase(1, "Hello world!")]
    public async Task ClientCanSendMessageAndServerBroadcastsToRoom(int roomId, string message)
    {
        var messagesSentToClient1 = new List<ServerBroadcastsMessageToClientsInRoom>();
        var messagesSentToClient2 = new List<ServerBroadcastsMessageToClientsInRoom>();
        var enterRoomEvent = new ClientWantsToEnterRoom
        {
            roomId = roomId
        };
        var sendMessageEvent = new ClientWantsToSendMessageToRoom
        {
            roomId = roomId,
            messageContent = message
        };

        using (var client = new WebsocketClient(_uri))
        using (var client2 = new WebsocketClient(_uri))
        {
            client.MessageReceived.Subscribe(message =>
            {
                Log.Information("CLIENT 1: GETS: " + message.Text);
                if (message.Text.Deserialize<BaseTransferObject>().eventType ==
                    nameof(ServerBroadcastsMessageToClientsInRoom))
                    messagesSentToClient1.Add(message.Text.Deserialize<ServerBroadcastsMessageToClientsInRoom>());
            });

            client2.MessageReceived.Subscribe(message =>
            {
                Log.Information("CLIENT 2: GETS: " + message.Text);
                if (message.Text.Deserialize<BaseTransferObject>().eventType ==
                    nameof(ServerBroadcastsMessageToClientsInRoom))
                    messagesSentToClient2.Add(message.Text.Deserialize<ServerBroadcastsMessageToClientsInRoom>());
            });

            await client.Start();
            await client2.Start();
            await Task.Delay(TimeSpan.FromSeconds(1));

            client.Send(enterRoomEvent.ToJsonString());
            client2.Send(enterRoomEvent.ToJsonString());
            client.Send(sendMessageEvent.ToJsonString());
            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        messagesSentToClient1
            .First().message.messageContent.Should().Be(message);
        messagesSentToClient2
            .First().message.messageContent.Should().Be(message);
    }
}