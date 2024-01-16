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
    public static Task DoAndWaitUntil<T>(this WebsocketClient ws,
        T action,
        List<Func<bool>> waitUntilConditionsAreMet,
        List<BaseDto>? communication = null) where T : BaseDto
    {
        communication?.Add(action);
        ws.Send(JsonSerializer.Serialize(action, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        }));
        var startTime = DateTime.UtcNow;
        while (waitUntilConditionsAreMet.All(x => !x.Invoke()))
        {
            var elapsedTime = DateTime.UtcNow - startTime;
            if (elapsedTime > TimeSpan.FromSeconds(2))
                throw new TimeoutException("Timeout. Unmet conditions");

            Task.Delay(100).Wait();
        }

        return Task.CompletedTask;
    }


    public static async Task<WebsocketClient> SetupWsClient(List<BaseDto> history)
    {
        Console.WriteLine("Starting websocket client on port " + Environment.GetEnvironmentVariable("FULLSTACK_API_PORT"));
        var ws = new WebsocketClient(new Uri("ws://localhost:" + Environment.GetEnvironmentVariable("FULLSTACK_API_PORT")));
        ws.MessageReceived.Subscribe(msg => { history.Add(msg.Text!.DeserializeAndValidate<BaseDto>()); });
        await ws.Start();
        return ws;
    }

    public static async Task SetupTestClass(PostgreSqlContainer pgcontainer)
    {
        await pgcontainer.StartAsync();
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("FULLSTACK_PG_CONN", pgcontainer.GetConnectionString());
        await new NpgsqlConnection(pgcontainer.GetConnectionString()).ExecuteAsync(StaticValues.DbRebuild);
        ApiStartup.StartApi().Wait();
    }
}