using api.ExtensionMethods;
using api.Reusables;
using api.ServerEvents;
using api.SharedApiModels;
using Infrastructure;
using JetBrains.Annotations;
using MediatR;

namespace api.ClientEvents;

public class ClientWantsToLoadOlderMessages : BaseTransferObject
{
    public int roomId { get; set; }
    public int lastMessageId { get; set; }
}

[UsedImplicitly]
public class ClientWantsToLoadOlderMessagesHandler(ChatRepository chatRepository)
    : IRequestHandler<EventTypeRequest<ClientWantsToLoadOlderMessages>>
{
    public Task Handle(EventTypeRequest<ClientWantsToLoadOlderMessages> request, CancellationToken cancellationToken)
    {
        SocketUtilities.ExitIfNotAuthenticated(request.Socket, request.MessageObject.eventType);
        var messages = chatRepository.GetPastMessages(
            request.MessageObject.roomId,
            request.MessageObject.lastMessageId);
        request.Socket.SendDto(new ServerSendsOlderMessagesToClient
            { messages = messages, roomId = request.MessageObject.roomId });
        return Task.CompletedTask;
    }
}