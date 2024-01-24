using api.Models;
using api.Models.ServerEvents;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.ApiGranularTests;

[TestFixture]
public class MustAuthenticateToEnterRoom
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
    public async Task WebSocket_Client_Must_Be_Authenticated_To_Enter_Room()
    {
        var history = new List<BaseDto>();
        var wsAndHistory = await StaticHelpers.SetupWsClient(history);
        await wsAndHistory.DoAndWaitUntil(StaticValues.EnterRoomEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerSendsErrorMessageToClient)) == 1,
            () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 0
        }, history);
    }
}