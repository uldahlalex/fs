using api.Models;
using Fleck;
using Infrastructure;

public interface IEventHandler<T> where T : BaseTransferObject
{
    public string EventType => GetType().Name;

    [EnforceName("DeserializeAndInvokeHandler")] //If rename, also rename invocation in handler (in websocket server)
    Task DeserializeAndInvokeHandler(string message, IWebSocketConnection socket);

    Task Handle(T dto, IWebSocketConnection socket);
}