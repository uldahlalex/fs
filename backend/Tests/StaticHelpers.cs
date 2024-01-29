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
}