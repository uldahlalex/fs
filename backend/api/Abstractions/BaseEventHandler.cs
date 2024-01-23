using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Authentication;
using System.Threading.RateLimiting;
using api.Attributes;
using api.Models;
using api.State;
using api.StaticHelpers.ExtensionMethods;
using Commons;
using Fleck;

namespace api.Abstractions;

public abstract class BaseEventHandler<T> where T : BaseDto
{
    public string eventType => GetType().Name;

    public async Task InvokeHandle(string message, IWebSocketConnection socket) //todo cancellationtoken
    {
        if (GetType().GetCustomAttributes<RequireAuthenticationAttribute>().Any())
            RequireAuthenticationAttribute.ValidateAuthentication(socket);
        if (GetType().GetCustomAttributes<RateLimitAttribute>().Any())
            await RateLimitAttribute.ValidateRateLimit(socket);
        
        await Handle(message.DeserializeAndValidate<T>(), socket);
    }

    public abstract Task Handle(T dto, IWebSocketConnection socket);
}