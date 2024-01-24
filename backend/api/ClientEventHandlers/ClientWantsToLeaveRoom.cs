using api.Abstractions;
using api.Attributes.EventFilters;
using api.Models;
using api.Models.Enums;
using api.Models.ServerEvents;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToLeaveRoomDto : BaseDto
{
    public int roomId { get; set; }
}

[RateLimit(600, 60)]
public class ClientWantsToLeaveRoom : BaseEventHandler<ClientWantsToLeaveRoomDto>
{
    public override Task Handle(ClientWantsToLeaveRoomDto request, IWebSocketConnection socket)
    {
        var isValidTopic = Enum.TryParse("ChatRoom" + request.roomId, out TopicEnums topic);
        if (!isValidTopic)
            throw new Exception("Invalid topic");
        socket.UnsubscribeFromTopic(topic);
        StaticWebSocketHelpers.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasLeftRoom
            { user = socket.GetMetadata().UserInfo }, topic);
        return Task.CompletedTask;
    }
}