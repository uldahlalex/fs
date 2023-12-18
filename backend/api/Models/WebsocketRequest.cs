using Fleck;
using MediatR;

namespace api.Models;

public class EventTypeRequest<BaseTransferObject> : IRequest
{
    public IWebSocketConnection Socket { get; set; }
    public BaseTransferObject MessageObject { get; set; }
}