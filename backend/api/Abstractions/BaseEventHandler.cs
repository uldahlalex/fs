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
        var dto = message.DeserializeAndValidate<T>();
        GetType().GetCustomAttributes().OfType<BaseEventFilterAttribute>().Select(x =>  x.Handle<T>(socket, dto));
        await Handle(dto, socket);
    }

    public abstract Task Handle(T dto, IWebSocketConnection socket);
}