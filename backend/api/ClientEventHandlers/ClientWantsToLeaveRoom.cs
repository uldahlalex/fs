using api.Abstractions;
using api.Helpers;
using api.Helpers.ExtensionMethods;
using api.Models;
using api.Models.Enums;
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
        bool isValidTopic = Enum.TryParse("ChatRoom" + request.roomId, out TopicEnums topic);
        if (!isValidTopic)
            throw new Exception("Invalid topic");
        socket.UnsubscribeFromTopic(topic);
        WebsocketHelpers.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
            { user = socket.GetMetadata().UserInfo }, topic);
        return Task.CompletedTask;
    }
}