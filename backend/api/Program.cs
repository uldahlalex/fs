using System.Reflection;
using api.Abstractions;
using api.Extensions;
using api.Externalities;
using api.Helpers;
using Fleck;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
    .CreateLogger();

EnvSetup.SetDefaultEnvVariables();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddNpgsqlDataSource(Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN")!,
    sourceBuilder => sourceBuilder.EnableParameterLogging());

builder.Services.AddSingleton<ChatRepository>();
builder.Services.AddSingleton<TimeSeriesRepository>();

builder.Services.AddSingleton<MqttClient>();

builder.AutomaticServiceAddFromBaseType(Assembly.GetExecutingAssembly(), typeof(BaseEventHandler<>));


var app = builder.Build();

var server = new WebSocketServer("ws://0.0.0.0:8181");

void Config(IWebSocketConnection ws)
{
    ws.OnOpen = ws.AddConnection;
    ws.OnClose = ws.RemoveFromConnections;
    ws.OnMessage = message => app.InvokeCorrectClientEventHandler(ws, message);
}

server.RestartAfterListenError = true;
server.ListenerSocket.NoDelay = true;
server.Start(Config);
var mqttClientSetting = Environment.GetEnvironmentVariable("FULLSTACK_START_MQTT_CLIENT")!;
if (!string.IsNullOrEmpty(mqttClientSetting) && mqttClientSetting.Equals("true", StringComparison.OrdinalIgnoreCase))
    await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message();
app.Run();