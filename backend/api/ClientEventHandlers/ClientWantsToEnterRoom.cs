using System.ComponentModel.DataAnnotations;
using api.Abstractions;
using api.Extensions;
using api.Externalities;
using api.Helpers;
using api.Helpers.Attributes;
using api.Models;
using api.Models.Enums;
using api.Models.ServerEvents;
using api.State;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToEnterRoomDto : BaseDto
{
    [Required] [Range(1, int.MaxValue)] public int roomId { get; set; }
}

[RequireAuthentication]
public class ClientWantsToEnterRoom(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToEnterRoomDto>
{
    public override Task Handle(ClientWantsToEnterRoomDto dto, IWebSocketConnection socket)
    {
        TopicEnums topic = dto.roomId.RoomIdToTopic();
        socket.SubscribeToTopic(topic);
        WebsocketHelpers.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
        {
            message = "Client joined the room!",
            user = socket.GetMetadata().UserInfo,
            roomId = dto.roomId
        }, topic);
        socket.SendDto(new ServerAddsClientToRoom
        {
            messages = chatRepository.GetPastMessages(dto.roomId),
            liveConnections = WebsocketConnections.TopicSubscriptions[topic].Count,
            roomId = dto.roomId
        });
        return Task.CompletedTask;
    }
    
}