using System.ComponentModel.DataAnnotations;
using api.Abstractions;
using api.Attributes.EventFilters;
using api.Models;
using api.Models.ServerEvents;
using api.State;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Externalities;
using Externalities.ParameterModels;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToEnterRoomDto : BaseDto
{
    [Required] [Range(1, int.MaxValue)] public int roomId { get; set; }
}

[RequireAuthentication]
[RateLimit(10, 60)]
public class ClientWantsToEnterRoom(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToEnterRoomDto>
{
    public override async Task Handle(ClientWantsToEnterRoomDto dto, IWebSocketConnection socket)
    {
        var topic = dto.roomId.ParseTopicFromRoomId();
        socket.SubscribeToTopic(topic);
        StaticWebSocketHelpers.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
        {
            message = "Client joined the room!",
            user = socket.GetMetadata().UserInfo,
            roomId = dto.roomId
        }, topic);
        socket.SendDto(new ServerAddsClientToRoom
        {
            messages = await chatRepository.GetPastMessages(new GetPastMessagesParams(dto.roomId)),
            liveConnections = WebsocketConnections.TopicSubscriptions[topic].Count,
            roomId = dto.roomId
        });
    }
}