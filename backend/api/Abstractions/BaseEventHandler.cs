using System.Reflection;
using System.Security.Authentication;
using api.Attributes;
using api.Models;
using api.State;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.Abstractions;

public abstract class BaseEventHandler<T> where T : BaseDto
{
    public string eventType => GetType().Name;

    public async Task InvokeHandle(string message, IWebSocketConnection socket)
    {
        if (GetType().GetCustomAttributes<RequireAuthenticationAttribute>().Any() && socket.IsAuthenticated())
                throw new AuthenticationException("Unauthorized access.");
        await Handle(message.DeserializeAndValidate<T>(), socket);
    }

    public abstract Task Handle(T dto, IWebSocketConnection socket);
}