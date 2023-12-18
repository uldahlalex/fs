using api.Models;
using Fleck;
using Infrastructure;
using Newtonsoft.Json;

public abstract class BaseEventHandler<T> : IEventHandler<T> where T : BaseTransferObject
{
    public string EventType => GetType().Name;

    // Centralized deserialization and invocation logic
    public async Task DeserializeAndInvokeHandler(string message, IWebSocketConnection socket)
    {
        var dto = JsonConvert.DeserializeObject<T>(message);
        if (dto == null)
        {
            throw new InvalidOperationException("Deserialization failed.");
        }

        await Handle(dto, socket);
    }

    // Abstract method to be implemented by derived classes
    public abstract Task Handle(T dto, IWebSocketConnection socket);
}



public interface IEventHandler<T> where T : BaseTransferObject
{
    string EventType => GetType().Name;

    Task Handle(T dto, IWebSocketConnection socket);

    // async Task DeserializeAndInvokeHandler(string message, IWebSocketConnection socket)
    // {
    //     var dto = JsonConvert.DeserializeObject<T>(message);
    //     if (dto == null)
    //     {
    //         throw new InvalidOperationException("Deserialization failed.");
    //     }
    //     
    //     await Handle(dto, socket);
    // }
}
