using System.ComponentModel.DataAnnotations;
using api.Abstractions;
using api.Externalities;
using api.Helpers;
using api.Helpers.Attributes;
using api.Helpers.ExtensionMethods;
using api.Models;
using api.Models.Enums;
using api.Models.ServerEvents;
using api.State;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToEnterRoomDto : BaseTransferObject
{
    [Required] [Range(1, int.MaxValue)] public int roomId { get; set; }
}

public class ClientWantsToEnterRoom(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToEnterRoomDto>
{
    [RequireAuthentication]
    public override Task Handle(ClientWantsToEnterRoomDto dto, IWebSocketConnection socket)
    {
        bool isValidTopic = Enum.TryParse("ChatRoom"+dto.roomId, out TopicEnums topic);
        if(!isValidTopic)
            throw new Exception("Invalid topic");
        WebsocketHelpers.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
        {
            message = "Client joined the room!",
            user = socket.GetMetadata().UserInfo,
            roomId = dto.roomId
        }, topic);
        socket.SubscribeToTopic(topic);
        socket.SendDto(new ServerAddsClientToRoom
        {
            messages = chatRepository.GetPastMessages(dto.roomId),
            liveConnections =
                WebsocketConnections.TopicSubscriptions[topic]
                    .Count, 
            roomId = dto.roomId
        });
        return Task.CompletedTask;
    }
}