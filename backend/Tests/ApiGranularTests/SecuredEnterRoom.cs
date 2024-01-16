using System.Text.Json;
using System.Xml;
using api.Models;
using api.Models.ServerEvents;
using Dapper;
using FluentAssertions;
using Npgsql;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Websocket.Client;

namespace Tests.ApiTests;

[TestFixture]
public class MustAuthenticateToEnterRoom
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    
    [Test]
    public async Task WebSocket_Client_Must_Be_Authenticated_To_Enter_Room()
    {
        var history = new List<BaseDto>();
        var wsAndHistory = await StaticHelpers.SetupWsClient(history);
        await wsAndHistory.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, new()
        {
            () => history.Count(x => x.eventType == nameof(ServerSendsErrorMessageToClient)) == 1,
            () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 0,
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