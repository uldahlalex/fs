using System.ComponentModel.DataAnnotations;
using dotenv.net;
using dotenv.net.Utilities;
using Serilog;

namespace api.Helpers;

public static class Tour
{
    private static readonly Dictionary<string, string> DefaultEnvValues = new Dictionary<string, string>
    {
        { "FULLSTACK_START_MQTT_CLIENT", "false" }, // Default value assumes MQTT client is not used
        { "FULLSTACK_JWT_PRIVATE_KEY", "YourJWTPrivateKey" },
        { "FULLSTACK_AZURE_COGNITIVE_SERVICES", "YourAzureKey" },
        { "FULLSTACK_PG_CONN", "YourPostgreSQLConnectionString" }
    };

    public static void CheckIsFirstRun()
    {
        try
        {
            DotEnv.Load();
        }
        catch (Exception ex)
        {
            Log.Warning("Error loading .env file: " + ex.Message);
            CreateOrUpdateEnvFileWithDefaults();
            Log.Information("A default .env file has been created. Please update it with your specific configurations.");
            return;
        }

        LoadEnvironmentVariables();
        Log.Information("Environment variables loaded. If you need to change settings, please edit the .env file directly.");
    }

    private static void LoadEnvironmentVariables()
    {
        foreach (var pair in DefaultEnvValues)
        {
            if (EnvReader.TryGetStringValue(pair.Key, out var value))
            {
                Environment.SetEnvironmentVariable(pair.Key, value);
            }
            else
            {
                // Handle missing or malformed .env file
                CreateOrUpdateEnvFileWithDefaults();
                Log.Warning($"Missing or invalid key in .env file: {pair.Key}. Default values have been set.");
                return;
            }
        }
    }

    private static void CreateOrUpdateEnvFileWithDefaults()
    {
        string envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
        var lines = new List<string>();

        foreach (var pair in DefaultEnvValues)
        {
            lines.Add($"{pair.Key}={pair.Value}");
        }

        File.WriteAllLines(envFilePath, lines);
    }
}
