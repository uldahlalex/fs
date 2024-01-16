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
        using (var ws = new WebsocketClient(new Uri(StaticHelpers.Url)))
        {
            var wsAndHistory = await ws.SetupWsClient();
            var wsAndHistory2 = await ws.SetupWsClient();

            await wsAndHistory.DoAndWaitUntil(StaticHelpers.AuthEvent, new()
                {
                    () => wsAndHistory.communication.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
                }
            );
            await wsAndHistory2.DoAndWaitUntil(StaticHelpers.AuthEvent, new()
                {
                    () => wsAndHistory2.communication.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
                }
            );
            await wsAndHistory.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, new()
            {
                () => wsAndHistory.communication.Count(x =>
                    x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 1,
                () => wsAndHistory.communication.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 1
            });
            await wsAndHistory2.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, new()
            {
                () => wsAndHistory2.communication.Count(x =>
                    x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 1,
                
            });
        }
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