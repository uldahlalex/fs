using FluentAssertions;
using NUnit.Framework;
using Websocket.Client;
using Newtonsoft.Json;
using api.Websocket;
using core;
using core.Models.WebsocketTransferObjects;
using core.TextTools;
using Infrastructure;
using Npgsql;

namespace YourNamespace.Tests;

public class WebsocketServerTests
{
    public WebsocketServerTests()
    {
        new WebsocketServer(
                new ChatRepository(
                    new NpgsqlDataSourceBuilder(
                            Utilities.ProperlyFormattedConnectionString).Build())).StartWebsocketServer();
    }

    [TestCase(1)]
    public async Task ShouldHandle_ClientWantsToEnterRoom_MessageCorrectly(int roomId)
    {
        var messageReceived = await MessageReceived(roomId);
        var actual = Deserializer<ServerNotifiesClientsInRoom>.Deserialize(
            messageReceived.receivedMessage);
        actual.Should().BeEquivalentTo(new ServerNotifiesClientsInRoom()
        {
            roomId = roomId,
            message = "A new user has entered the room!"
        });
    }

    [TestCase(1)]
    public async Task ShouldHandle_ClientWantsToEnterRoom_MessageCorrectly_GetsResponse(int roomId)
    {
        var messageReceived = await MessageReceived(roomId);
        messageReceived.messageReceived.Should().BeTrue();
    }


    private async Task<(bool messageReceived, string receivedMessage)> MessageReceived(int roomId)
    {
        //todo make tests that have X assertions but arrange + act is extracted

        // Arrange
        var url = new Uri("ws://localhost:8181");
        using var client = new WebsocketClient(url);

        var enterRoomMessage = new ClientWantsToEnterRoom
        {
            roomId = roomId
        };

        var messageReceived = false;
        var receivedMessage = string.Empty;

        client.MessageReceived.Subscribe(message =>
        {
            messageReceived = true;
            receivedMessage = message.Text;
        });

        await client.Start();

        // Act
        client.Send(JsonConvert.SerializeObject(enterRoomMessage));

        // Wait for the message to be received by the client
        await Task.Delay(TimeSpan.FromSeconds(1));
        return (messageReceived, receivedMessage);
    }
}