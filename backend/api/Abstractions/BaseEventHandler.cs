using System.Reflection;
using api.Attributes;
using api.Models;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.Abstractions;

public abstract class BaseEventHandler<T> where T : BaseDto
{
    public string eventType => GetType().Name;

    public async Task InvokeHandle(string message, IWebSocketConnection socket)
    {
        if (GetType().GetCustomAttributes<RequireAuthenticationAttribute>().Any())
            socket.ExitIfNotAuthenticated();
        await Handle(message.DeserializeAndValidate<T>(), socket);
    }

    public abstract Task Handle(T dto, IWebSocketConnection socket);
}