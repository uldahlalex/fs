using api.Resusables;
using api.ServerEvents;
using api.SharedApiModels;
using core.Attributes;
using core.ExtensionMethods;
using core.Models.QueryModels;
using Infrastructure;
using JetBrains.Annotations;
using MediatR;

namespace api.ClientEvents;

public class ClientWantsToSendMessageToRoom : BaseTransferObject
{
    [ToxicityFilter] public string? messageContent { get; set; }

    public int roomId { get; set; }
}

[UsedImplicitly]
public class ClientWantsToSendMessageToRoomHandler(ChatRepository chatRepository)
    : IRequestHandler<EventTypeRequest<ClientWantsToSendMessageToRoom>>
{
    public Task Handle(EventTypeRequest<ClientWantsToSendMessageToRoom> request, CancellationToken cancellationToken)
    {
        Reusables.ExitIfNotAuthenticated(request.Socket, request.MessageObject.eventType);
        var insertedMessage =
            chatRepository.InsertMessage(request.MessageObject.roomId, request.Socket.GetMetadata().userInfo.id,
                request.MessageObject.messageContent!);
        var messageWithUserInfo = new MessageWithSenderEmail
        {
            room = insertedMessage.room,
            sender = insertedMessage.sender,
            timestamp = insertedMessage.timestamp,
            messageContent = insertedMessage.messageContent,
            id = insertedMessage.id,
            email = request.Socket.GetMetadata().userInfo.email
        };
        WebsocketExtensions.BroadcastObjectToTopicListeners(new ServerBroadcastsMessageToClientsInRoom
        {
            message = messageWithUserInfo,
            roomId = request.MessageObject.roomId
        }, request.MessageObject.roomId.ToString());
        return Task.CompletedTask;
    }
}