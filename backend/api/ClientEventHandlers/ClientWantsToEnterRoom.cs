using System.ComponentModel.DataAnnotations;
using api.ExtensionMethods;
using api.Helpers;
using api.Models;
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
    public override Task Handle(ClientWantsToEnterRoomDto dto, IWebSocketConnection socket)
    {
        SocketUtilities.ExitIfNotAuthenticated(socket, dto.eventType);
        SocketUtilities.BroadcastObjectToTopicListeners(new ServerNotifiesClientsInRoomSomeoneHasJoinedRoom
        {
            message = "Client joined the room!",
            user = socket.GetMetadata().UserInfo,
            roomId = dto.roomId
        }, "ChatRooms/" + dto.roomId);
        socket.SubscribeToTopic("ChatRooms/" + dto.roomId);
        socket.SendDto(new ServerAddsClientToRoom
        {
            messages = chatRepository.GetPastMessages(dto.roomId),
            liveConnections =
                WebsocketConnections.TopicSubscriptions["ChatRooms/" + dto.roomId]
                    .Count, //socket.CountUsersInRoom(dto.roomId.ToString()),
            roomId = dto.roomId
        });
        return Task.CompletedTask;
    }
}