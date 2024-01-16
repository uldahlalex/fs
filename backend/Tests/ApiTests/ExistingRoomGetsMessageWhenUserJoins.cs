using api.Models;
using api.Models.ServerEvents;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Websocket.Client;

namespace Tests.ApiTests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class ExistingRoomGetsMessageWhenUserJoins
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [Test]
    public async Task MessageWhenUserJoins()
    {
        var history = new List<BaseDto>();
        using var ws = await StaticHelpers.SetupWsClient(history);
        using var ws2 = await StaticHelpers.SetupWsClient(history);

        await ws.DoAndWaitUntil(StaticHelpers.AuthEvent, history, new()
            {
                () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
            }
        );
        await ws2.DoAndWaitUntil(StaticHelpers.AuthEvent, history, new()
            {
                () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
            }
        );
        await ws.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, history,new()
        {
            () => history.Count(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 1,
            () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 1
        });
        await ws2.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, history, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 3,
            () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 2
                
        });
        
        await ws.DoAndWaitUntil(StaticHelpers.SendMessageEvent, history, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)) == 2,
        });
        await ws2.DoAndWaitUntil(StaticHelpers.SendMessageEvent, history, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)) == 4,
        });
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