using api;
using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.ApiBigBangTests;

[TestFixture]
public class AuthEnterSend
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
    public async Task Client_Can_Authenticate_Enter_Room_And_Send_Message()
    {
        var history = new List<BaseDto>();
        var wsAndHistory = await StaticHelpers.SetupWsClient(history);


        await wsAndHistory.DoAndWaitUntil(StaticValues.AuthEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
        }, history);
        await wsAndHistory.DoAndWaitUntil(StaticValues.EnterRoomEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 1
        }, history);

        await wsAndHistory.DoAndWaitUntil(StaticValues.SendMessageEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)) == 1
        }, history);
    }
}