using System.Reflection;
using api.Helpers;
using api.Helpers.Attributes;
using api.Models;
using Fleck;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace api.Abstractions;

public abstract class BaseEventHandler<T> where T : BaseTransferObject
{
    
    [UsedImplicitly] //Used for method invocation by the event handler manager
    public string eventType => GetType().Name;
    
    [UsedImplicitly]
    public Task DeserializeAndInvokeHandler(string message, IWebSocketConnection socket)
    {
        var dto = JsonConvert.DeserializeObject<T>(message);
        if (dto == null)
        {
            throw new InvalidOperationException("Deserialization failed.");
        }
        var handleMethod = GetType().GetMethod("Handle");
        var requiresAuth = handleMethod!.GetCustomAttribute<RequireAuthenticationAttribute>() != null;
        if (requiresAuth)
        {
            WebsocketHelpers.ExitIfNotAuthenticated(socket, eventType); 
        }

        return Handle(dto, socket);
    }

    public abstract Task Handle(T dto, IWebSocketConnection socket);
}