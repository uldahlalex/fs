using System.Reflection;
using api.Models;
using Commons;
using Fleck;

namespace api.StaticHelpers.ExtensionMethods;

public static class ReflectionExtensions
{
    public static HashSet<Type> AddServiceAndReturnAll(this WebApplicationBuilder builder, Assembly assemblyReference,
        Type genericTypeDefinition)
    {
        var clientEventHandlers = new HashSet<Type>();
        foreach (var type in assemblyReference.GetTypes())
            if (type.BaseType != null &&
                type.BaseType.IsGenericType &&
                type.BaseType.GetGenericTypeDefinition() == genericTypeDefinition)
            {
                builder.Services.AddSingleton(type);
                clientEventHandlers.Add(type);
            }

        return clientEventHandlers;
    }

    public static async Task InvokeCorrectClientEventHandler(this WebApplication app, HashSet<Type> types, IWebSocketConnection ws, string message)
    {
        var eventType = message.Deserialize<BaseDto>().eventType;
        var handlerType = types.FirstOrDefault(t => t.Name.Equals(eventType, StringComparison.OrdinalIgnoreCase));
        if (handlerType == null)
            throw new InvalidOperationException($"Could not find handler for DTO type: {eventType}");
        dynamic clientEventServiceClass = app.Services.GetService(handlerType)!;
        await clientEventServiceClass.InvokeHandle(message, ws);
    }
}