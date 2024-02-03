using api.ClientEventHandlers;
using api.Models.ServerEvents;
using api.StaticHelpers;
using lib;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.ApiBigBangTests;

[TestFixture]
public class AuthEnterSendToxTrigger
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await StaticHelpers.SetupTestClass(_postgreSqlContainer);
    }


    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [Test]
    public async Task ToxicityFilterRejectsMessage()
    {
        var client = await new WebSocketTestClient().ConnectAsync();
        await client.DoAndAssert(StaticValues.AuthEvent,
            receivedMessages =>
            {
                return receivedMessages.Any(x => x.eventType.Equals(nameof(ServerAuthenticatesUser)));
            });


        await client.DoAndAssert(StaticValues.EnterRoomEvent, receivedMessages =>
        {
            return receivedMessages.Any(x => x.eventType.Equals(nameof(ServerAddsClientToRoom))) &&
                   receivedMessages.Any(x =>
                       x.eventType.Equals(nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)));
        });

        await client.DoAndAssert(new ClientWantsToSendMessageToRoomDto
        {
            messageContent = "I hate you",
            roomId = 1
        }, receivedMessages =>
        {
            return receivedMessages.Count(x => x.eventType.Equals(nameof(ServerBroadcastsMessageToClientsInRoom))) == 0
                   && receivedMessages.Any(x => x.eventType.Equals(nameof(ServerSendsErrorMessageToClient)));
        });


        client.Client.Dispose();
    }
}