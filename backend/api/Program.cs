using System.Reflection;
using api;
using api.Abstractions;
using api.ClientEventHandlers;
using api.Externalities;
using api.Helpers;
using api.Helpers.Attributes;
using api.Models.DbModels;
using Npgsql;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
    .CreateLogger();

EnvSetup.SetDefaultEnvVariables();

EnforceNameCheck.CheckPropertyNames<EndUser>();
EnforceNameCheck.CheckPropertyNames<Message>();
EnforceNameCheck.CheckPropertyNames<Room>();
EnforceNameCheck.CheckPropertyNames<UserRoomJunctions>();
EnforceNameCheck.CheckPropertyNames<TimeSeries>();

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddNpgsqlDataSource(Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN")!,
    sourceBuilder => sourceBuilder.EnableParameterLogging());

builder.Services.AddSingleton<ChatRepository>();
builder.Services.AddSingleton<TimeSeriesRepository>();

builder.Services.AddSingleton<WebsocketServer>();
builder.Services.AddSingleton<MqttClient>();

// builder.Services.AddSingleton<ClientWantsToAuthenticate>();
// builder.Services.AddSingleton<ClientWantsToAuthenticateWithJwt>();
// builder.Services.AddSingleton<ClientWantsToRegister>();
//
// builder.Services.AddSingleton<ClientWantsToEnterRoom>();
// builder.Services.AddSingleton<ClientWantsToLeaveRoom>();
// builder.Services.AddSingleton<ClientWantsToLoadOlderMessages>();
// builder.Services.AddSingleton<ClientWantsToSendMessageToRoom>();
//
// builder.Services.AddSingleton<ClientWantsToKnowWhatTopicsTheySubscribeTo>();
//
// builder.Services.AddSingleton<ClientWantsToSubscribeToTimeSeriesData>();

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

var app = builder.Build();

try
{
    app.Services.GetService<NpgsqlDataSource>().OpenConnection().Close();
} catch (Exception e)
{
    Log.Error(e, "Postgres connection failed. Exiting.");
    Log.Information("PGCONN: "+Environment.GetEnvironmentVariable("FULLSTACK_PG_CONN"));
}


WebsocketServer.HandlerTypes = handlerTypes;

var mqttClientSetting = Environment.GetEnvironmentVariable("FULLSTACK_START_MQTT_CLIENT")!;
if (!string.IsNullOrEmpty(mqttClientSetting) && mqttClientSetting.Equals("true", StringComparison.OrdinalIgnoreCase))
   await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message();

app.Services.GetService<WebsocketServer>()!.StartWebsocketServer();
await app.RunAsync();

