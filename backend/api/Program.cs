using api;
using api.ClientEventHandlers;
using api.Models;
using Fleck;
using Infrastructure;
using Infrastructure.DbModels;
using MediatR;
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
builder.Services.AddSingleton<Mediator>();
builder.Services.AddSingleton<ClientWantsToAuthenticate>();
builder.Services.AddSingleton<EventHandlerService>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
//add MediatR
//builder.Services.AddMediatR(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();
try
{
    // Console.WriteLine(Environment.GetEnvironmentVariable("START_BROKER"));
    // if (Environment.GetEnvironmentVariable("START_BROKER")!.Equals("true") || true)
    //todo start med cli arg i stedet for env var
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
    if (Environment.GetEnvironmentVariable("tox")!.Length == 0)
        Log.Information("No tox environment variable present to connect to Azure cognitive service toxicity filter");
    if (Environment.GetEnvironmentVariable("secret")!.Length == 0)
        Log.Information("No secret environment variable present to issue and validate JWTs");
    if (Environment.GetEnvironmentVariable("pgconn")!.Length == 0)
        Log.Information("Not pgconn environment variable present to connect to postgresql");
}
catch (Exception e)
{
    Log.Error(e, "Program.cs");
}

app.Services.GetService<WebsocketServer>()!.StartWebsocketServer();
await app.RunAsync();

public interface IEventHandler<T> where T : BaseTransferObject
{
    public string EventType => GetType().Name;

    Task HandleAsync(string message, IWebSocketConnection socket);

    Task Handle(T dto, IWebSocketConnection socket);
}

public class EventHandlerService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _handlerMap;

    public EventHandlerService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _handlerMap = new Dictionary<string, Type>
        {
            { "ClientWantsToAuthenticate", typeof(ClientWantsToAuthenticate) },
            // Map other event types to handlers...
        };
    }

    public async Task HandleEventAsync(string eventType, string message, IWebSocketConnection socket)
    {
        if (_handlerMap.TryGetValue(eventType, out var handlerType))
        {
            var handler = (dynamic)_serviceProvider.GetRequiredService(handlerType);
            await handler.HandleAsync(message, socket);
        }
        else
        {
            throw new InvalidOperationException($"Handler not found for event type: {eventType}");
        }
    }
}