using api.ExtensionMethods;
using api.Helpers;
using api.Models;
using api.Models.ServerEvents;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToLeaveRoomDto : BaseTransferObject
{
    public int roomId { get; set; }
}

public class ClientWantsToLeaveRoom : BaseEventHandler<ClientWantsToLeaveRoomDto>
{
    public override Task Handle(ClientWantsToLeaveRoomDto request, IWebSocketConnection socket)
    {
        socket.UnsubscribeFromTopic(request.roomId.ToString());
        SocketUtilities.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
                { user = socket.GetMetadata().UserInfo },
            request.roomId.ToString());
        return Task.CompletedTask;
    }
}