using api;
using Infrastructure;

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