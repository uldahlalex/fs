using api.Attributes.EventFilters;
using api.Models.ServerEvents;
using api.StaticHelpers.ExtensionMethods;
using Externalities;
using Externalities.ParameterModels;
using Fleck;
using lib;

namespace api.ClientEventHandlers;

public class ClientWantsToLoadOlderMessagesDto : BaseDto
{
    public int roomId { get; set; }
    public int lastMessageId { get; set; }
}

[RequireAuthentication]
[RateLimit(5, 20)]
public class ClientWantsToLoadOlderMessages(ChatRepository chatRepository)
    : BaseEventHandler<ClientWantsToLoadOlderMessagesDto>
{
    public override async Task Handle(ClientWantsToLoadOlderMessagesDto dto, IWebSocketConnection socket)
    {
        var messages =  chatRepository.GetPastMessages(new GetPastMessagesParams(dto.roomId, dto.lastMessageId));
        socket.SendDto(new ServerSendsOlderMessagesToClient
            { messages = messages, roomId = dto.roomId });
    }
}