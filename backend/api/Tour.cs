using dotenv.net;
using dotenv.net.Utilities;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class Tour
{
    public static void CheckIsFirstRun()
    {
        try
        {
            
            DotEnv.Load(); 
        }
        catch (Exception ex)
        {
            Log.Warning("Error loading .env file: " + ex.Message);
            // Optionally, recreate a new .env file here if needed
        }

        bool hasValue = EnvReader.TryGetStringValue("FULLSTACK_QUESTIONNAIRE_COMPLETED", out var firstRun);
        if (hasValue && firstRun.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            LoadEnvironmentVariables();
            Log.Information(@"
App has already been configured. Skipping configuration questionnaire.
If you wish to go through the questionnaire again, please set the FULLSTACK_QUESTIONNAIRE_COMPLETED to false or delete the line the .env file.");
            return;
        }
    
        ConductConfigurationQuestionnaire();
        UpdateEnvFile("FULLSTACK_QUESTIONNAIRE_COMPLETED", "true");
    }

    private static void LoadEnvironmentVariables()
    {
        var variables = new List<string> { "FULLSTACK_PG_CONN", "FULLSTACK_JWT_PRIVATE_KEY", "FULLSTACK_AZURE_COGNITIVE_SERVICES", "FULLSTACK_START_MQTT_CLIENT" };
        foreach (var variable in variables)
        {
            if (EnvReader.TryGetStringValue(variable, out var value))
            {
                Environment.SetEnvironmentVariable(variable, value);
            }
        }
    }

    private static void ConductConfigurationQuestionnaire()
    {
        AskQuestionAndUpdateEnv("Do you wish to connect to the MQTT client? (y/N)\n(Assumes broker runs localhost port 1883)", "FULLSTACK_START_MQTT_CLIENT");
        AskQuestionAndUpdateEnv("Enter a private key for JWT token generation\n(if empty, uses env variable FULLSTACK_JWT_PRIVATE_KEY):", "FULLSTACK_JWT_PRIVATE_KEY");
        AskQuestionAndUpdateEnv("Enter your Azure Cognitive Services key\n(if empty, uses env variable FULLSTACK_AZURE_COGNITIVE_SERVICES):", "FULLSTACK_AZURE_COGNITIVE_SERVICES");
        AskQuestionAndUpdateEnv("Enter your PostgreSQL connection string\n(if empty, uses env variable FULLSTACK_PG_CONN):", "FULLSTACK_PG_CONN");
    }

    private static void AskQuestionAndUpdateEnv(string question, string envKey)
    {
        Log.Information(question);
        var response = Console.ReadLine();
        if (!string.IsNullOrEmpty(response))
        {
            var value = envKey == "FULLSTACK_START_MQTT_CLIENT" && response.ToLower().Equals("y") ? "true" : response;
            UpdateEnvFile(envKey, value);
            Environment.SetEnvironmentVariable(envKey, value);
        }
    }
    

    private static void UpdateEnvFile(string key, string value)
    {
        string envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
  

        var lines = new List<string>();
        bool keyExists = false;

        if (File.Exists(envFilePath))
        {
            lines = File.ReadAllLines(envFilePath).ToList();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].StartsWith(key + "="))
                {
                    keyExists = true;
                    lines[i] = $"{key}={value}"; // Update existing key
                    break;
                }
            }
        }

        if (!keyExists)
        {
            lines.Add($"{key}={value}"); // Add new key-value pair
        }

        File.WriteAllLines(envFilePath, lines);
    }
}
