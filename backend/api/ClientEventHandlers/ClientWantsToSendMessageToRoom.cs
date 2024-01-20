using api.Abstractions;
using api.Attributes;
using api.Externalities;
using api.Models;
using api.Models.DbModels;
using api.Models.Enums;
using api.Models.QueryModels;
using api.Models.ServerEvents;
using api.State;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToSendMessageToRoomDto : BaseDto
{
    [ToxicityFilter] public string? messageContent { get; set; }

    public int roomId { get; set; }
}

[RequireAuthentication]
public class ClientWantsToSendMessageToRoom(ChatRepository chatRepository)
    : BaseEventHandler<ClientWantsToSendMessageToRoomDto>
{
    public override Task Handle(ClientWantsToSendMessageToRoomDto dto, IWebSocketConnection socket)
    {
        var topic = dto.roomId.ParseTopicFromRoomId();
        var getValue = WebsocketConnections.TopicSubscriptions.TryGetValue(topic,
            out var topicSubscriptions);
        if (!getValue || !topicSubscriptions!.Contains(socket.ConnectionInfo.Id))
            throw new Exception("You are not subscribed to this room");
        var message =new Message
        {
            timestamp = DateTimeOffset.UtcNow,
            room = dto.roomId,
            sender = socket.GetMetadata().UserInfo.id,
            messageContent = dto.messageContent!
        };
        var insertedMessage = chatRepository.InsertMessage(message);
        var messageWithUserInfo = new MessageWithSenderEmail //todo this is kinda ugly ngl
        {
            room = insertedMessage.room,
            sender = insertedMessage.sender,
            timestamp = insertedMessage.timestamp,
            messageContent = insertedMessage.messageContent,
            id = insertedMessage.id,
            email = socket.GetMetadata().UserInfo.email
        };
        StaticWebSocketHelpers.BroadcastObjectToTopicListeners(new ServerBroadcastsMessageToClientsInRoom
        {
            message = messageWithUserInfo,
            roomId = dto.roomId
        }, topic);
        return Task.CompletedTask;
    }
}