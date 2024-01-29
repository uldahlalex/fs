using System.Reflection;
using System.Text.Json;
using api;
using api.Abstractions;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Commons;
using Dapper;
using Externalities;
using Fleck;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Serilog;

var app = await ApiStartup.StartApi();
app.Run();

namespace api
{
    public static class ApiStartup
    {
        public static async Task<WebApplication> StartApi()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
                .CreateLogger();

            EnvSetup.SetDefaultEnvVariables();

            Log.Information(JsonSerializer.Serialize(Environment.GetEnvironmentVariables(), new JsonSerializerOptions
            {
                WriteIndented = true
            }));

            var builder = WebApplication.CreateBuilder();

            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            if (builder.Environment.IsProduction())
            {
                builder.Services.AddNpgsqlDataSource(
                    Utilities.ProperlyFormattedConnectionString!);
            }
            else
            {
                builder.Services.AddNpgsqlDataSource(Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN")!,
                    sourceBuilder => sourceBuilder.EnableParameterLogging());
            }


            builder.Services.AddSingleton<ChatRepository>();
            builder.Services.AddSingleton<TimeSeriesRepository>();

            builder.Services.AddSingleton<MqttClient>();
            builder.Services.AddSingleton<AzureCognitiveServices>();

            var types = builder.AddServiceAndReturnAll(Assembly.GetExecutingAssembly(), typeof(BaseEventHandler<>));


            var app = builder.Build();
            if (Environment.GetCommandLineArgs().Contains("--rebuild-db"))
                Utilities.ExecuteRebuildFromSqlScript();
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? throw new Exception("Not found");
            if (string.IsNullOrEmpty(env))
                throw new Exception("Must have ASPNETCORE_ENVIRONMENT");
            var port = env.Equals(EnvironmentEnums.Development.ToString())
                ? 8181
                : 0;
            var server = new WebSocketServer("ws://0.0.0.0:"+port);

            void Config(IWebSocketConnection ws)
            {
                ws.OnOpen = ws.AddConnection;
                ws.OnClose = ws.RemoveFromWebsocketConnections;
                ws.OnError = ex => ex.Handle(ws, null);
                ws.OnMessage = async message =>
                {
                    if (!app.Environment.IsProduction()) Log.Information(message);
                    try
                    {
                        await app.InvokeCorrectClientEventHandler(types, ws, message);
                    }
                    catch (Exception ex)
                    {
                        ex.Handle(ws, message);
                    }
                };
            }

            server.RestartAfterListenError = true;
            server.Start(Config);

            Environment.SetEnvironmentVariable("FULLSTACK_API_PORT", server.Port.ToString());
            var mqttClientSetting = Environment.GetEnvironmentVariable("FULLSTACK_START_MQTT_CLIENT")!;
            if (!string.IsNullOrEmpty(mqttClientSetting) &&
                mqttClientSetting.Equals("true", StringComparison.OrdinalIgnoreCase))
                await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message();
            
            app.MapGet("/", async ([FromServices] AzureCognitiveServices az) =>  await az.File());
            return app;
        }
    }
}