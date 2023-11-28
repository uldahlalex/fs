using api.Mqtt;
using api.Websocket;
using core.Attributes;
using core.Models;
using Infrastructure;
using Serilog;

EnforceNameCheck.CheckPropertyNames<EndUser>();
EnforceNameCheck.CheckPropertyNames<Message>();
EnforceNameCheck.CheckPropertyNames<Room>();
EnforceNameCheck.CheckPropertyNames<UserRoomJunctions>();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddNpgsqlDataSource(Utilities.ProperlyFormattedConnectionString,
    sourceBuilder => sourceBuilder.EnableParameterLogging());
builder.Services.AddSingleton<ChatRepository>();
builder.Services.AddSingleton<WebsocketServer>();
builder.Services.AddSingleton<MqttClient>();
var app = builder.Build();
app.Services.GetService<WebsocketServer>()!.StartWebsocketServer();
//await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message(); //if broker aint running it throws
await app.RunAsync();