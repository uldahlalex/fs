using api.Models.ServerEvents;
using api.StaticHelpers;
using lib;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.ApiBigBangTests;

[TestFixture]
public class AuthEnterSend
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await StaticHelpers.SetupTestClass(_postgreSqlContainer, true, true);
    }


    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [Test]
    [Repeat(10)]
    public async Task Client_Can_Authenticate_Enter_Room_And_Send_Message()
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

        await client.DoAndAssert(StaticValues.SendMessageEvent,
            receivedMessages =>
            {
                return receivedMessages.Count(x =>
                    x.eventType.Equals(nameof(ServerBroadcastsMessageToClientsInRoom))) == 1;
            });


        client.Client.Dispose();
    }
}