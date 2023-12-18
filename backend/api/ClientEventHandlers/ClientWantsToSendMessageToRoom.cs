using api.Attributes;
using api.ExtensionMethods;
using api.Helpers;
using api.Models;
using api.Models.ServerEvents;
using api.State;
using Fleck;
using Infrastructure;
using Infrastructure.QueryModels;
using JetBrains.Annotations;

namespace api.ClientEventHandlers;

public class ClientWantsToSendMessageToRoomDto : BaseTransferObject
{
    [ToxicityFilter] 
    public string? messageContent { get; set; }

    public int roomId { get; set; }
}

public class ClientWantsToSendMessageToRoom(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToSendMessageToRoomDto>
{
    public override Task Handle(ClientWantsToSendMessageToRoomDto dto, IWebSocketConnection socket)
    {
        SocketUtilities.ExitIfNotAuthenticated(socket, dto.eventType);
        var getValue = WebsocketConnections.TopicSubscriptions.TryGetValue("ChatRooms/" + dto.roomId,
            out var topicSubscriptions);
        if (!getValue || !topicSubscriptions.Contains(socket.ConnectionInfo.Id))
            throw new Exception("You are not subscribed to this room");


        var insertedMessage =
            chatRepository.InsertMessage(dto.roomId, socket.GetMetadata().UserInfo.id,
                dto.messageContent!);
        var messageWithUserInfo = new MessageWithSenderEmail
        {
            room = insertedMessage.room,
            sender = insertedMessage.sender,
            timestamp = insertedMessage.timestamp,
            messageContent = insertedMessage.messageContent,
            id = insertedMessage.id,
            email = socket.GetMetadata().UserInfo.email
        };
        SocketUtilities.BroadcastObjectToTopicListeners(new ServerBroadcastsMessageToClientsInRoom
        {
            message = messageWithUserInfo,
            roomId = dto.roomId
        }, "ChatRooms/" + dto.roomId);
        return Task.CompletedTask;
    }
}