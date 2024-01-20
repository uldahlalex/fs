using System.Text.Json;
using api;
using api.Models;
using api.StaticHelpers.ExtensionMethods;
using Dapper;
using Npgsql;
using Testcontainers.PostgreSql;
using Websocket.Client;

namespace Tests;

public static class StaticHelpers
{
    public static async Task SetupTestClass(PostgreSqlContainer pgContainer)
    {
        await pgContainer.StartAsync();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("FULLSTACK_PG_CONN", pgContainer.GetConnectionString());
        await new NpgsqlConnection(pgContainer.GetConnectionString()).ExecuteAsync(StaticValues.DbRebuild);
        ApiStartup.StartApi().Wait();
    }

    public static Task<WebsocketClient> SetupWsClient(List<BaseDto> history)
    {
        Console.WriteLine("Starting websocket client on port " +
                          Environment.GetEnvironmentVariable("FULLSTACK_API_PORT"));
        var ws = new WebsocketClient(new Uri("ws://localhost:" +
                                             Environment.GetEnvironmentVariable("FULLSTACK_API_PORT")));
        ws.MessageReceived.Subscribe(msg => { history.Add(msg.Text!.DeserializeAndValidate<BaseDto>()); });
        ws.Start().Wait();
        if (!ws.IsRunning) throw new InvalidOperationException("Failed to establish a WebSocket connection.");
        return Task.FromResult(ws);
    }

    public static Task DoAndWaitUntil<T>(this WebsocketClient ws,
        T action,
        List<Func<bool>> waitUntilConditionsAreMet,
        List<BaseDto> communication,
        string? failureToDoMessage = null) where T : BaseDto
    {
        communication.Add(action);
        ws.Send(JsonSerializer.Serialize(action, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        }));
        var startTime = DateTime.UtcNow;
        while (waitUntilConditionsAreMet.Any(x => !x.Invoke()))
        {
            var elapsedTime = DateTime.UtcNow - startTime;
            if (elapsedTime > TimeSpan.FromSeconds(2))
                throw new TimeoutException("Timeout. Unmet conditions " + failureToDoMessage);

            Task.Delay(100).Wait();
        }

        return Task.CompletedTask;
    }
}