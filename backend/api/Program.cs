using api.Mqtt;
using api.Websocket;
using core;
using core.Attributes;
using core.Models;
using Infrastructure;
using MQTTnet.Exceptions;
using Serilog;


EnforceNameCheck.CheckPropertyNames<EndUser>();
EnforceNameCheck.CheckPropertyNames<Message>();
EnforceNameCheck.CheckPropertyNames<Room>();
EnforceNameCheck.CheckPropertyNames<UserRoomJunctions>();
EnforceNameCheck.CheckPropertyNames<TimeSeriesDataPoint>();

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
    app.Services.GetService<WebsocketServer>()!.StartWebsocketServer();
    var startBroker = Environment.GetEnvironmentVariable("START_BROKER");
    Console.WriteLine("START_BROKER env variable is: " + startBroker);
    if (startBroker is "true")
        await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message();
    await app.RunAsync();
}
catch (MQTTnet.Exceptions.MqttCommunicationException e)
{
    Log.Error(e, "MQTT broker not started!");
}
catch (Exception e)
{
    Log.Error(e, "Failed to start services");
}
