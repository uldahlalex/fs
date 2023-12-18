using System.Reflection;
using api;
using api.ClientEventHandlers;
using Fleck;
using Infrastructure;
using Infrastructure.DbModels;
using MQTTnet.Exceptions;
using Serilog;
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
    .CreateLogger();
Tour.CheckIsFirstRun();
EnforceNameCheck.CheckPropertyNames<EndUser>();
EnforceNameCheck.CheckPropertyNames<Message>();
EnforceNameCheck.CheckPropertyNames<Room>();
EnforceNameCheck.CheckPropertyNames<UserRoomJunctions>();
EnforceNameCheck.CheckPropertyNames<TimeSeries>();



var builder = WebApplication.CreateBuilder(args);
builder.Services.AddNpgsqlDataSource(Utilities.ProperlyFormattedConnectionString,
    sourceBuilder => sourceBuilder.EnableParameterLogging());
builder.Services.AddSingleton<ChatRepository>();
builder.Services.AddSingleton<TimeSeriesRepository>();
builder.Services.AddSingleton<WebsocketServer>();
builder.Services.AddSingleton<MqttClient>();
builder.Services.AddSingleton<ClientWantsToAuthenticate>();
builder.Services.AddSingleton<ClientWantsToEnterRoom>();

var handlerTypes = new List<Type>();
foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
{
    if (type.BaseType != null && type.BaseType.IsGenericType && 
        type.BaseType.GetGenericTypeDefinition() == typeof(BaseEventHandler<>))
    {
        builder.Services.AddSingleton(type);
        handlerTypes.Add(type);
    }
}

// After all services have been registered
var app = builder.Build();

// Store the list of handler types in a static property or pass it where needed
WebsocketServer.HandlerTypes = handlerTypes;

string mqttClientSetting = Environment.GetEnvironmentVariable("FULLSTACK_START_MQTT_CLIENT");
if (mqttClientSetting != null && mqttClientSetting.Equals("true", StringComparison.OrdinalIgnoreCase))
{
   await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message();
}
app.Services.GetService<WebsocketServer>()!.StartWebsocketServer();
await app.RunAsync();

