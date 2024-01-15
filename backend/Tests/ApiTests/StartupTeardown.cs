using System.Text.Json;
using System.Xml;
using api.Models.ServerEvents;
using Dapper;
using FluentAssertions;
using Npgsql;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Websocket.Client;

namespace Tests.ApiTests;

[TestFixture]
public class ApiTests
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();
    
    [Test]
    public async Task Signin_EnterRoom_SendMessage()
    {
        using (var ws = new WebsocketClient(new Uri(StaticHelpers.Url)))
        {
            var wsAndHistory = await ws.SetupWsClient();


            await wsAndHistory.DoAndWaitUntil(StaticHelpers.AuthEvent, new ()
                {
                   () => wsAndHistory.communication.Count(x => x.eventType == nameof(ServerAuthenticatesUser)) == 1
                     

                }
            );
            await wsAndHistory.DoAndWaitUntil(StaticHelpers.EnterRoomEvent,  new()
            {
                () => wsAndHistory.communication.Count(x => x.eventType == nameof(ServerAddsClientToRoom)) == 1

            });

            await wsAndHistory.DoAndWaitUntil(StaticHelpers.SendMessageEvent,  new()
            { 
                () => wsAndHistory.communication.Count(x => x.eventType == nameof(ServerBroadcastsMessageToClientsInRoom)) == 1

            });

            wsAndHistory.communication.Should().Contain(x => x.eventType == nameof(ServerAuthenticatesUser));

        }
    }

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        await _postgreSqlContainer.StartAsync();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("FULLSTACK_TEST_PG_CONN", _postgreSqlContainer.GetConnectionString()); //todo
        using (var conn = new NpgsqlConnection(_postgreSqlContainer.GetConnectionString()))
        {
            conn.Execute(StaticHelpers.DbRebuild);
            var result = conn.Query("SELECT * FROM chat.enduser;");
            TestContext.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
            }));
        }
        await ApiStartup.StartApi(new string[0]);

    }
    

    [OneTimeTearDown]
    public async Task DisposeAsync()
    {
        await _postgreSqlContainer.DisposeAsync();
    }
    
    
}