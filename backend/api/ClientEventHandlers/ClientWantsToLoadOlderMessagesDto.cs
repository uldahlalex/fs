using api.Abstractions;
using api.Attributes;
using api.ExtensionMethods;
using api.Helpers;
using api.Models;
using api.Models.ServerEvents;
using Fleck;
using Infrastructure;
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