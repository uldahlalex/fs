using api.Abstractions;
using api.Extensions;
using api.Externalities;
using api.Helpers;
using api.Helpers.Attributes;
using api.Models;
using api.Models.Enums;
using api.Models.QueryModels;
using api.Models.ServerEvents;
using api.State;
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
        var chatRoomName = "ChatRoom" + dto.roomId;
        var isValidTopic = Enum.TryParse(chatRoomName, out TopicEnums topic);
        if (!isValidTopic)
            throw new Exception("Invalid topic");
        var getValue = WebsocketConnections.TopicSubscriptions.TryGetValue(topic,
            out var topicSubscriptions);
        if (!getValue || !topicSubscriptions!.Contains(socket.ConnectionInfo.Id))
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
        WebsocketHelpers.BroadcastObjectToTopicListeners(new ServerBroadcastsMessageToClientsInRoom
        {
            message = messageWithUserInfo,
            roomId = dto.roomId
        }, topic);
        return Task.CompletedTask;
    }
}