using api.Models;
using Fleck;

namespace api.Abstractions;

public abstract class BaseEventFilterAttribute : Attribute
{
    public abstract Task Handle<T>(IWebSocketConnection socket, T dto) where T : BaseDto;
}