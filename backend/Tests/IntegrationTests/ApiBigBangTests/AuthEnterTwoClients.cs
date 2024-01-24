using api.Models;
using api.Models.ServerEvents;
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
    public async Task Existing_Clients_In_Room_Get_Joined_Message_When_Someone_Enters()
    {
        var history = new List<BaseDto>();
        var ws1 = await StaticHelpers.SetupWsClient(history);
        var ws2 = await StaticHelpers.SetupWsClient(history);


        await ws1.DoAndWaitUntil(StaticValues.AuthEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
        }, history);
        await ws2.DoAndWaitUntil(StaticValues.AuthEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 2
        }, history);
        await ws1.DoAndWaitUntil(StaticValues.EnterRoomEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 1,
            () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 1
        }, history);
        await ws2.DoAndWaitUntil(StaticValues.EnterRoomEvent, new List<Func<bool>>
        {
            () => history.Count(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 3,
            () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 2
        }, history);
    }
}