using api.SharedApiModels;
using core.ExtensionMethods;
using Fleck;

namespace api.Resusables;

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