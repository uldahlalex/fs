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
[Parallelizable(ParallelScope.All)]
public class ApiTests
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    
    [Test]
    public async Task Signin_EnterRoom_SendMessage()
    {

        var history = new List<BaseDto>();
        var wsAndHistory = await StaticHelpers.SetupWsClient(history);


            await wsAndHistory.DoAndWaitUntil(StaticHelpers.AuthEvent, history, new ()
                {
                   () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
                     

                }
            );
            await wsAndHistory.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, history, new()
            {
                () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 1,
            });

            await wsAndHistory.DoAndWaitUntil(StaticHelpers.SendMessageEvent,  history,new()
            { 
                () => history.Count(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)) == 1
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