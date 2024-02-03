using api;
using Commons;
using Externalities;
using Testcontainers.PostgreSql;

namespace Tests;

public static class StaticHelpers
{
    public static async Task SetupTestClass(PostgreSqlContainer pgContainer, bool skipRateLimit = false,
        bool skipToxFilter = false)
    {
        await pgContainer.StartAsync();
        var connectionString = pgContainer.GetConnectionString();
        connectionString += ";Pooling=true;MinPoolSize=1;MaxPoolSize=100;";
        Environment.SetEnvironmentVariable("FULLSTACK_PG_CONN", connectionString);
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentEnums.Testing.ToString());
        Environment.SetEnvironmentVariable("FULLSTACK_SKIP_RATE_LIMITING", skipRateLimit.ToString().ToLower());
        Environment.SetEnvironmentVariable("FULLSTACK_SKIP_TOX_FILTER", skipToxFilter.ToString().ToLower());
        Utilities.ExecuteRebuildFromSqlScript();
        await ApiStartup.StartApi();
    }
}