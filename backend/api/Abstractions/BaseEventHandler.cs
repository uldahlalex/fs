using System.Reflection;
using api.Models;
using Commons;
using Fleck;

namespace api.Abstractions;

public abstract class BaseEventHandler<T> where T : BaseDto
{
    public string eventType => GetType().Name;

    public async Task InvokeHandle(string message, IWebSocketConnection socket) //todo cancellationtoken
    {
        var dto = message.Deserialize<T>();
        await dto.ValidateAsync();
        foreach (var baseEventFilterAttribute in GetType().GetCustomAttributes().OfType<BaseEventFilterAttribute>())
            await baseEventFilterAttribute.Handle(socket, dto);
        await Handle(dto, socket);
    }

    public abstract Task Handle(T dto, IWebSocketConnection socket);
}