using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers;
using FluentAssertions;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.ApiBigBangTests;

[TestFixture]
public class AuthEnterBroadcast
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
    public async Task Auth_Enter_BroadCast_Two_Clients_Works()
    {
        var client = await new WebSocketTestClient().ConnectAsync();
        var client2 = await new WebSocketTestClient().ConnectAsync();
        await client.DoAndAssert(StaticValues.AuthEvent, receivedMessages =>
        {
            return receivedMessages.Any(x => x.eventType.Equals(nameof(ServerAuthenticatesUser)));
        });
        
        await client2.DoAndAssert(StaticValues.AuthEvent, receivedMessages =>
        {
            return receivedMessages.Any(x => x.eventType.Equals(nameof(ServerAuthenticatesUser)));
        });
        await client.DoAndAssert(StaticValues.EnterRoomEvent, receivedMessages =>
        {
            return receivedMessages.Any(x => x.eventType.Equals(nameof(ServerAddsClientToRoom))) &&
                   receivedMessages.Any(x =>
                       x.eventType.Equals(nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)));
        });
        await client2.DoAndAssert(StaticValues.EnterRoomEvent, receivedMessages =>
        {
            return receivedMessages.Any(x => x.eventType.Equals(nameof(ServerAddsClientToRoom))) &&
                   receivedMessages.Count(x => x.eventType.Equals(nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)))==1;
        });
        await client.DoAndAssert(StaticValues.SendMessageEvent, receivedMessages =>
        {
            return receivedMessages.Count(x => x.eventType.Equals(nameof(ServerBroadcastsMessageToClientsInRoom))) == 1;
        });
        await client2.DoAndAssert(StaticValues.SendMessageEvent, receivedMessages =>
        {
            return receivedMessages.Count(x => x.eventType.Equals(nameof(ServerBroadcastsMessageToClientsInRoom))) == 2;
        });


        client.Client.Dispose();
        client2.Client.Dispose();
    }
}