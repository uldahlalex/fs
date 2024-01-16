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
public class AuthEnterSend
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    
    [Test]
    public async Task Client_Can_Authenticate_Enter_Room_And_Send_Message()
    {

        var history = new List<BaseDto>();
        var wsAndHistory = await StaticHelpers.SetupWsClient(history);


            await wsAndHistory.DoAndWaitUntil(StaticHelpers.AuthEvent, new ()
                {
                    () => history.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
                     

                }, history);
            await wsAndHistory.DoAndWaitUntil(StaticHelpers.EnterRoomEvent, new()
            {
                () => history.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 1,
            }, history);

            await wsAndHistory.DoAndWaitUntil(StaticHelpers.SendMessageEvent,new()
            { 
                () => history.Count(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)) == 1
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