using System.Reflection;
using api.Extensions;
using api.Helpers;
using api.Helpers.Attributes;
using api.Models;
using Fleck;

namespace api.Abstractions;

public abstract class BaseEventHandler<T> where T : BaseDto
{
    public string eventType => GetType().Name;

    public Task InvokeHandle(string message, IWebSocketConnection socket)
    {
        if (ReferenceEquals(GetType().GetCustomAttributes<RequireAuthenticationAttribute>(), null))
            socket.ExitIfNotAuthenticated(eventType);
        try
        {
            return Handle(message.DeserializeAndValidate<T>(), socket);
        }
        catch (Exception e)
        {
            GeneralExceptionHandler.Handle(e, socket, eventType, message);
            return Task.CompletedTask;
        }
    }

    public abstract Task Handle(T dto, IWebSocketConnection socket);
}