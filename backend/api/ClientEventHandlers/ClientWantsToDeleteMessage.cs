using api.Abstractions;
using api.Attributes.EventFilters;
using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers;
using api.StaticHelpers.ExtensionMethods;
using Externalities;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToDeleteMessageDto : BaseDto
{
    public int messageId { get; set; }
    public int roomId { get; set; }
}

[RateLimit(10, 60)]
[RequireAuthentication]
public class ClientWantsToDeleteMessage(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToDeleteMessageDto>
{
    public override async Task Handle(ClientWantsToDeleteMessageDto dto, IWebSocketConnection socket)
    {
        var user = socket.GetMetadata().UserInfo;
        if (!user.isadmin)
        {
            var isOwner = chatRepository.IsMessageOwner(new IsMessageOwnerParams(user.id, dto.messageId));
            if (isOwner)
                throw new UnauthorizedAccessException(
                    "You must be either admin or owner of the message to delete it");
        }

        chatRepository.DeleteMessage(new DeleteMessageParams() {messageId = dto.messageId});
        StaticWebSocketHelpers.BroadcastObjectToTopicListeners(new ServerDeletesMessage()
        {
            messageId = dto.messageId,
            roomId = dto.roomId
        }, dto.roomId.ParseTopicFromRoomId());
    }
}