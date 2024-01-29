using System.Text.Json;
using api;
using api.Models;
using api.StaticHelpers;
using Commons;
using Dapper;
using Externalities;
using Npgsql;
using NUnit.Framework;
using Testcontainers.PostgreSql;
using Websocket.Client;

namespace Tests;

public static class StaticHelpers
{
    public static async Task SetupTestClass(PostgreSqlContainer pgContainer, bool skipRateLimit = false, bool skipToxFilter = false)
    {
        await pgContainer.StartAsync();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentEnums.Testing.ToString());
        Environment.SetEnvironmentVariable("FULLSTACK_SKIP_RATE_LIMITING", skipRateLimit.ToString().ToLower());
        Environment.SetEnvironmentVariable("FULLSTACK_SKIP_TOX_FILTER", skipToxFilter.ToString().ToLower());
        Environment.SetEnvironmentVariable("FULLSTACK_PG_CONN", pgContainer.GetConnectionString());
        Utilities.ExecuteRebuildFromSqlScript();
        await ApiStartup.StartApi();
    }

    public static async Task<WebsocketClient> SetupWsClient(List<BaseDto> history)
    {
        Console.WriteLine("Starting websocket client on port " +
                          Environment.GetEnvironmentVariable("FULLSTACK_API_PORT"));
        var ws = new WebsocketClient(new Uri("ws://localhost:" +
                                             Environment.GetEnvironmentVariable("FULLSTACK_API_PORT")));
        ws.MessageReceived.Subscribe(msg => { history.Add(msg.Text!.Deserialize<BaseDto>()); });
        await ws.Start();
        if (!ws.IsRunning) throw new InvalidOperationException("Failed to establish a WebSocket connection.");
        return ws;
    }

    public static Task DoAndWaitUntil<T>(this WebsocketClient ws,
        T action,
        List<Func<bool>> waitUntilConditionsAreMet,
        List<BaseDto> communication,
        string? failureToDoMessage = null) where T : BaseDto
    {
        communication.Add(action);
        ws.SendInstant(JsonSerializer.Serialize(action, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        })).Wait();
        var startTime = DateTime.UtcNow;
        while (waitUntilConditionsAreMet.Any(x => !x.Invoke()))
        {
            var elapsedTime = DateTime.UtcNow - startTime;
            if (elapsedTime > TimeSpan.FromSeconds(2))
            {
                TestContext.WriteLine
                ("The full event history: " + JsonSerializer.Serialize(communication,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    }));
                throw new TimeoutException("Timeout. Unmet conditions " + failureToDoMessage);
            }


            Task.Delay(100).Wait();
        }

        return Task.CompletedTask;
    }
}