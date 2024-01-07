using System.Reflection;
using api.Models;
using Fleck;

namespace api.Extensions;

public static class UtilityExtensions
{
    public static void AutomaticServiceAddFromBaseType(this WebApplicationBuilder builder, Assembly assemblyReference,
        Type genericTypeDefinition)
    {
        foreach (var type in assemblyReference.GetTypes())
            if (type.BaseType != null &&
                type.BaseType.IsGenericType &&
                type.BaseType.GetGenericTypeDefinition() == genericTypeDefinition)
                builder.Services.AddSingleton(type);
    }

    //todo refactor to make agnostic towrads types
    public static async void InvokeCorrectClientEventHandler(this WebApplication app, IWebSocketConnection ws,
        string message)
    {
        var eventType = message.DeserializeAndValidate<BaseDto>().eventType;
        var handlerType = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .FirstOrDefault(t => t.Name.Equals(eventType, StringComparison.OrdinalIgnoreCase));
        if (handlerType == null)
            throw new InvalidOperationException($"Could not find handler for DTO type: {eventType}");
        dynamic handler = app.Services.GetService(handlerType)!;
        await handler.InvokeHandle(message, ws);
    }
}