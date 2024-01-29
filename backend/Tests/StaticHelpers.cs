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
        string connectionString = pgContainer.GetConnectionString();
        connectionString += ";Pooling=true;MinPoolSize=1;MaxPoolSize=100;";
        Environment.SetEnvironmentVariable("FULLSTACK_PG_CONN", connectionString);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentEnums.Testing.ToString());
        Environment.SetEnvironmentVariable("FULLSTACK_SKIP_RATE_LIMITING", skipRateLimit.ToString().ToLower());
        Environment.SetEnvironmentVariable("FULLSTACK_SKIP_TOX_FILTER", skipToxFilter.ToString().ToLower());
        Utilities.ExecuteRebuildFromSqlScript();
        await ApiStartup.StartApi();
    }
}