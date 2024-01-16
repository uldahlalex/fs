using api.Extensions;
using api.Models;
using api.Models.ServerEvents;
using FluentAssertions;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Websocket.Client;

namespace Tests.ApiTests;

[TestFixture]
[Parallelizable(ParallelScope.All)]
public class ExistingClientsInRoomGetJoinedMessageWhenSomeoneEnters
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    
    [Test]
    public async Task Existing_Clients_In_Room_Get_Joined_Message_When_Someone_Enters()
    {
    var history = new List<BaseDto>();
    var ws1 = await StaticHelpers.SetupWsClient(history);
    var ws2 = await StaticHelpers.SetupWsClient(history);


    await ws1.DoAndWaitUntil(StaticHelpers.AuthEvent, history, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
        }
    );
    await ws2.DoAndWaitUntil(StaticHelpers.AuthEvent, history, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
        }
    );
    await ws1.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, history,new()
    {
        () => history.Count(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 1,
        () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 1
    });
    await ws2.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, history, new()
    {
        () => history.Count(x => x.eventType == nameof(ServerNotifiesClientsInRoomSomeoneHasJoinedRoom)) == 3,
        () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 2
                
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