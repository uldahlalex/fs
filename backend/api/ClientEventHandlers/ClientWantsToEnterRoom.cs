using System.ComponentModel.DataAnnotations;
using api.Abstractions;
using api.Attributes;
using api.ExtensionMethods;
using api.Helpers;
using api.Models;
using api.Models.Enums;
using api.Models.ServerEvents;
using api.State;
using Fleck;
using Infrastructure;
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
                    .Count, //socket.CountUsersInRoom(dto.roomId.ToString()),
            roomId = dto.roomId
        });
        return Task.CompletedTask;
    }
}