using api.ServerEvents;
using api.SharedApiModels;
using core.ExtensionMethods;
using JetBrains.Annotations;
using MediatR;

namespace api.ClientEvents;

public class ClientWantsToLeaveRoom : BaseTransferObject
{
    public int roomId { get; set; }
}

[UsedImplicitly]
public class ClientWantsToLeaveRoomHandler : IRequestHandler<EventTypeRequest<ClientWantsToLeaveRoom>>
{
    public Task Handle(EventTypeRequest<ClientWantsToLeaveRoom> request, CancellationToken cancellationToken)
    {
        request.Socket.UnsubscribeFromTopic(request.MessageObject.roomId.ToString());
        WebsocketExtensions.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
                { user = request.Socket.GetMetadata().userInfo },
            request.MessageObject.roomId.ToString());
        return Task.CompletedTask;
    }
}