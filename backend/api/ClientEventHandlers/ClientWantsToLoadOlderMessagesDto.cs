using api.Abstractions;
using api.Attributes;
using api.Externalities;
using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToLoadOlderMessagesDto : BaseDto
{
    public int roomId { get; set; }
    public int lastMessageId { get; set; }
}

[RequireAuthentication]
public class ClientWantsToLoadOlderMessages(ChatRepository chatRepository)
    : BaseEventHandler<ClientWantsToLoadOlderMessagesDto>
{
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