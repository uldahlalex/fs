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
builder.Services.AddSingleton<Events>();
builder.Services.AddSingleton<ChatRepository>();
builder.Services.AddSingleton<WebsocketServer>();
var app = builder.Build();
app.Services.GetService<WebsocketServer>()!.StartWebsocketServer();
app.Run();