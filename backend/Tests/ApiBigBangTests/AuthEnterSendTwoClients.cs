using api.Models;
using api.Models.ServerEvents;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.ApiTests;

[TestFixture]
public class AuthEnterSendTwoClients
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [Test]
    public async Task Two_Clients_Can_Authenticate_Enter_Room_Broadcast_Messages()
    {
        var history = new List<BaseDto>();
        using var ws = await StaticHelpers.SetupWsClient(history);
        using var ws2 = await StaticHelpers.SetupWsClient(history);

        await ws.DoAndWaitUntil(StaticHelpers.AuthEvent, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
        }, history);
        await ws2.DoAndWaitUntil(StaticHelpers.AuthEvent, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
        }, history);
        await ws.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 1,
            () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 1
        }, history);
        await ws2.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 3,
            () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 2
        }, history);

        await ws.DoAndWaitUntil(StaticHelpers.SendMessageEvent, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)) == 2,
        }, history);
        await ws2.DoAndWaitUntil(StaticHelpers.SendMessageEvent, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)) == 4,
        }, history);
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await StaticHelpers.Setup(_postgreSqlContainer);
    }


    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
}