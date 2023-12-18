using api.Abstractions;
using api.Externalities;
using api.Helpers;
using api.Helpers.Attributes;
using api.Helpers.ExtensionMethods;
using api.Models;
using api.Models.ServerEvents;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToLoadOlderMessagesDto : BaseTransferObject
{
    public int roomId { get; set; }
    public int lastMessageId { get; set; }
}

public class ClientWantsToLoadOlderMessages(ChatRepository chatRepository) : BaseEventHandler<ClientWantsToLoadOlderMessagesDto>
{
    [RequireAuthentication]
    public override Task Handle(ClientWantsToLoadOlderMessagesDto dto, IWebSocketConnection socket)
    {
        var messages = chatRepository.GetPastMessages(
            dto.roomId,
            dto.lastMessageId);
        socket.SendDto(new ServerSendsOlderMessagesToClient
            { messages = messages, roomId = dto.roomId });
        return Task.CompletedTask;
    }
}