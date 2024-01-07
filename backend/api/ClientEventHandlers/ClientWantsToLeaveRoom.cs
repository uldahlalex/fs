using api.Abstractions;
using api.Extensions;
using api.Helpers;
using api.Helpers.Attributes;
using api.Models;
using api.Models.Enums;
using api.Models.ServerEvents;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToLeaveRoomDto : BaseDto
{
    public int roomId { get; set; }
}

[RequireAuthentication] //todo make role based (only used can leave own connections room)
public class ClientWantsToLeaveRoom : BaseEventHandler<ClientWantsToLeaveRoomDto>
{
    public override Task Handle(ClientWantsToLeaveRoomDto request, IWebSocketConnection socket)
    {
        var isValidTopic = Enum.TryParse("ChatRoom" + request.roomId, out TopicEnums topic);
        if (!isValidTopic)
            throw new Exception("Invalid topic");
        socket.UnsubscribeFromTopic(topic);
        WebsocketHelpers.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
            { user = socket.GetMetadata().UserInfo }, topic);
        return Task.CompletedTask;
    }
}