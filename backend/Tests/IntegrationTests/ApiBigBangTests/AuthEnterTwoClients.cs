using api.Models.ServerEvents;
using api.StaticHelpers;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.ApiBigBangTests;

[TestFixture]
public class AuthEnterTwoClients
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
    [Repeat(1)]
    public async Task Existing_Clients_In_Room_Get_Joined_Message_When_Someone_Enters()
    {
        var client = await new WebSocketTestClient().ConnectAsync();
        var client2 = await new WebSocketTestClient().ConnectAsync();

         await client.DoAndWaitUntil(StaticValues.AuthEvent, new List<string>() {nameof(ServerAuthenticatesUser)});

         await client2.DoAndWaitUntil(StaticValues.AuthEvent, new List<string>() {nameof(ServerAuthenticatesUser)});


         await client.DoAndWaitUntil(StaticValues.EnterRoomEvent, new List<string>() {nameof(ServerAddsClientToRoom), nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)});
         await client2.DoAndWaitUntil(StaticValues.EnterRoomEvent, new List<string>() {nameof(ServerAddsClientToRoom), nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)});

        client.Client.Dispose();
        client2.Client.Dispose();
    }
}