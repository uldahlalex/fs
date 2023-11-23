using api;
using Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
    .CreateLogger();
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddNpgsqlDataSource(Utilities.ProperlyFormattedConnectionString,
    sourceBuilder => sourceBuilder.EnableParameterLogging());
builder.Services.AddSingleton<State>();
builder.Services.AddSingleton<AuthUtilities>();
builder.Services.AddSingleton<WebsocketUtilities>();
builder.Services.AddSingleton<ClientInducedEvents>();
builder.Services.AddSingleton<ChatRepository>();
builder.Services.AddSingleton<WebsocketServer>();
builder.Services.AddSingleton<MqttClient>();
//here you can also add an mqtt server
var app = builder.Build();
await app.Services.GetService<MqttClient>()!.Handle_Received_Application_Message();
await app.RunAsync();