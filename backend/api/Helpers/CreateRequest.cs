using api.ExtensionMethods;
using api.Models;
using Fleck;

namespace api.Helpers;

public static class RequestFactory
{
    public static EventTypeRequest<T> CreateRequest<T>(string message, IWebSocketConnection socket)
        where T : BaseTransferObject
    {
        return new EventTypeRequest<T>
        {
            Socket = socket,
            MessageObject = message.DeserializeToModelAndValidate<T>()
        };
    }
}