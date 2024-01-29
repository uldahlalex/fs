using api;
using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers;
using NUnit.Framework;
using Testcontainers.PostgreSql;

namespace Tests.IntegrationTests.ApiGranularTests;

[TestFixture]
public class MustAuthenticateToEnterRoom
{
    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await StaticHelpers.SetupTestClass(_postgreSqlContainer, true, true);
    }


    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }

    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    [Test]
    [Repeat(50)]
    public async Task WebSocket_Client_Must_Be_Authenticated_To_Enter_Room()
    {
        var client = await new WebSocketTestClient().ConnectAsync();
        await client.DoAndAssert(StaticValues.SendMessageEvent, dtos =>
        {
            return dtos.Any(x => x.eventType.Equals(nameof(ServerSendsErrorMessageToClient))) 
                   && dtos.Count(x => x.eventType.Equals(nameof(ServerAuthenticatesUser))) == 0
                   && dtos.Count(x => x.eventType.Equals(nameof(ServerAddsClientToRoom))) == 0
                   && dtos.Count(x => x.eventType.Equals(nameof(ServerBroadcastsMessageToClientsInRoom))) == 0;
        });
       
        
        client.Client.Dispose();
    }
}