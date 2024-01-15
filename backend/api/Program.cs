using System.Reflection;
using System.Text.Json;
using api.Abstractions;
using api.Extensions;
using api.Externalities;
using api.Helpers;
using Dapper;
using Fleck;
using Npgsql;
using Serilog;

var app = await ApiStartup.StartApi(args);
app.Run();

public class ApiStartup
{
    public static async Task<WebApplication> StartApi(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(
                outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
            .CreateLogger();

        EnvSetup.SetDefaultEnvVariables();

        var builder = WebApplication.CreateBuilder(args);

        // if (builder.Environment.IsEnvironment("Testing"))
        builder.Services.AddNpgsqlDataSource(Environment.GetEnvironmentVariable("FULLSTACK_TEST_PG_CONN")!,
            sourceBuilder => sourceBuilder.EnableParameterLogging());

        //
        // if (builder.Environment.IsDevelopment())
        // {
        //     builder.Services.AddNpgsqlDataSource(Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN")!,
        //                 sourceBuilder => sourceBuilder.EnableParameterLogging());
        // }


        builder.Services.AddSingleton<ChatRepository>();
        builder.Services.AddSingleton<TimeSeriesRepository>();

        builder.Services.AddSingleton<MqttClient>();

        var types = builder.AddServiceAndReturnAll(Assembly.GetExecutingAssembly(), typeof(BaseEventHandler<>));


        var app = builder.Build();


        var server = new WebSocketServer("ws://0.0.0.0:8181");

        void Config(IWebSocketConnection ws)
        {
            ws.OnOpen = ws.AddConnection;
            ws.OnClose = ws.RemoveFromWebsocketConnections;
            ws.OnError = ex => ex.Handle(ws, null);
            ws.OnMessage = async message =>
            {
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

        var q = app.Services.GetService<NpgsqlDataSource>().OpenConnection().Query("select * from chat.enduser");
        Console.WriteLine("FROM PROGRAM.CS"+JsonSerializer.Serialize(q, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
        server.RestartAfterListenError = true;
        server.ListenerSocket.NoDelay = true;
        server.Start(Config);
        var mqttClientSetting = Environment.GetEnvironmentVariable("FULLSTACK_START_MQTT_CLIENT")!;
        if (!string.IsNullOrEmpty(mqttClientSetting) &&
            mqttClientSetting.Equals("true", StringComparison.OrdinalIgnoreCase))
            await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message();
        return app;
    }
}