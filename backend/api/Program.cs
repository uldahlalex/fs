using System.Reflection;
using api.Abstractions;
using api.Externalities;
using api.Models.Enums;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Fleck;
using Serilog;

var app = await ApiStartup.StartApi();
app.Run();
public static class ApiStartup
{
    public static async Task<WebApplication> StartApi()
    {        Log.Logger = new LoggerConfiguration()
                 .WriteTo.Console(
                     outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
                 .CreateLogger();

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        Log.Information("ENVIRONMENT: " + environment);


        EnvSetup.SetDefaultEnvVariables();

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        builder.Services.AddNpgsqlDataSource(Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN")!,
            sourceBuilder => sourceBuilder.EnableParameterLogging());


        builder.Services.AddSingleton<ChatRepository>();
        builder.Services.AddSingleton<TimeSeriesRepository>();

        builder.Services.AddSingleton<MqttClient>();

        var types = builder.AddServiceAndReturnAll(Assembly.GetExecutingAssembly(), typeof(BaseEventHandler<>));


        var app = builder.Build();
        var port = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")!.Equals(EnvironmentEnums.Testing.ToString()) ? 0 : 8181;
        var server = new WebSocketServer("ws://0.0.0.0:" + port);

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
        server.ListenerSocket.NoDelay = true;
        server.Start(Config);
        Environment.SetEnvironmentVariable("FULLSTACK_API_PORT", server.Port.ToString());
        var mqttClientSetting = Environment.GetEnvironmentVariable("FULLSTACK_START_MQTT_CLIENT")!;
        if (!string.IsNullOrEmpty(mqttClientSetting) &&
            mqttClientSetting.Equals("true", StringComparison.OrdinalIgnoreCase))
            await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message();
        return app;
    }
}