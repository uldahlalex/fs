using Serilog;

namespace api.Helpers;

public static class EnvSetup
{
    private static bool IsRunningInDocker()
    {
        return File.Exists("/.dockerenv");
    }

    
    private static readonly Dictionary<string, string> DefaultEnvValues = new()
    {
        { "FULLSTACK_START_MQTT_CLIENT", "false" },
        { "FULLSTACK_JWT_PRIVATE_KEY", "YourJWTPrivateKey" },
        { "FULLSTACK_AZURE_COGNITIVE_SERVICES", "YourAzureKey" },
        {
            "FULLSTACK_PG_CONN", IsRunningInDocker()
                ? "Server=db;Database=postgres;User Id=postgres;Password=postgres;Port=5432;Pooling=true;MaxPoolSize=3"
                :
            "Server=localhost;Database=postgres;User Id=postgres;Password=postgres;Port=5432;Pooling=true;MaxPoolSize=3"
        }
    };

    public static void SetDefaultEnvVariables()
    {
        foreach (var pair in DefaultEnvValues)
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(pair.Key)))
            {
                Environment.SetEnvironmentVariable(pair.Key, pair.Value);
                Log.Information($"Environment variable '{pair.Key}' not found. Setting default value.");
            }
            else
            {
                Log.Information(
                    $"Environment variable '{pair.Key}' already set with value: '{Environment.GetEnvironmentVariable(pair.Key)}'. Skipping default value.");
            }
    }
}