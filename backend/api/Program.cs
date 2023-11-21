using api;
using Infrastructure;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Serilog.Sinks.SystemConsole.Themes;
/*
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(outputTemplate: "\n{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}\n")
    .CreateLogger();
    
    */
//todo 
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), "D:\\Temp\\Serilogs\\structuredLog.json", rollingInterval: RollingInterval.Day, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.With(enrichers: new ThreadIdEnricher())
    .Enrich.WithThreadId()
    .Enrich.WithProperty("ApplicationName", "Serilogs DemoApplication")
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddNpgsqlDataSource(Utilities.ProperlyFormattedConnectionString,
    sourceBuilder => sourceBuilder.EnableParameterLogging());


builder.Services.AddExceptionHandler<DeserializationExceptionHandler>();
builder.Services.AddSingleton<State>();
builder.Services.AddSingleton<AuthUtilities>();
builder.Services.AddSingleton<WebsocketUtilities>();
builder.Services.AddSingleton<Events>();
builder.Services.AddSingleton<ChatRepository>();
builder.Services.AddSingleton<FleckServer>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
//app.UseExceptionHandler(_ => {});
app.UseMiddleware<GlobalExceptionHandler>();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Services.GetService<FleckServer>()!.Start();
app.Run();
