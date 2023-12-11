using api.Mqtt;
using api.Websocket;
using core;
using core.Attributes;
using core.Models.DbModels;
using Infrastructure;
using MQTTnet.Exceptions;
using Serilog;

EnforceNameCheck.CheckPropertyNames<EndUser>();
EnforceNameCheck.CheckPropertyNames<Message>();
EnforceNameCheck.CheckPropertyNames<Room>();
EnforceNameCheck.CheckPropertyNames<UserRoomJunctions>();
EnforceNameCheck.CheckPropertyNames<TimeSeries>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddNpgsqlDataSource(Utilities.ProperlyFormattedConnectionString,
    sourceBuilder => sourceBuilder.EnableParameterLogging());
builder.Services.AddSingleton<ChatRepository>();
builder.Services.AddSingleton<TimeSeriesRepository>();
builder.Services.AddSingleton<WebsocketServer>();
builder.Services.AddSingleton<MqttClient>();
var app = builder.Build();
try
{
    // Console.WriteLine(Environment.GetEnvironmentVariable("START_BROKER"));
    // if (Environment.GetEnvironmentVariable("START_BROKER")!.Equals("true") || true)
    await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message();
}
catch (MqttCommunicationException e)
{
    Log.Information(e, "MQTT broker not started! Running anyways.");
}
catch (Exception e)
{
    Log.Error(e, "MQTT Broker exception");
}

try
{
    Log.Information("They environment variables required:");
    if (Environment.GetEnvironmentVariable("tox")!.Length==0)
    {
        Log.Information("No tox environment variable present to connect to Azure cognitive service toxicity filter");
    }
    if (Environment.GetEnvironmentVariable("secret")!.Length==0)
    {
        Log.Information("No secret environment variable present to issue and validate JWTs");
    }
    if (Environment.GetEnvironmentVariable("pgconn")!.Length==0)
    {
        Log.Information("Not pgconn environment variable present to connect to postgresql");
    }
}
catch (Exception e)
{
    Log.Error(e, "Program.cs");
}

app.Services.GetService<WebsocketServer>()!.StartWebsocketServer();
await app.RunAsync();