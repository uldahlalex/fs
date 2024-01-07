using api.Abstractions;
using api.Extensions;
using api.Models;
using api.Models.ServerEvents;
using api.State;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToKnowWhatTopicsTheySubscribeToDto : BaseDto;

public class
    ClientWantsToKnowWhatTopicsTheySubscribeTo : BaseEventHandler<ClientWantsToKnowWhatTopicsTheySubscribeToDto>
{
    public override Task Handle(ClientWantsToKnowWhatTopicsTheySubscribeToDto dto, IWebSocketConnection socket)
    {
        var list = WebsocketConnections.TopicSubscriptions
            .Where(x => x.Value.Contains(socket.ConnectionInfo.Id))
            .Select(x => x.Key)
            .ToList();
        socket.SendDto(new ServerSendsListOfTopicsClientSubscribesTo
        {
            topics = list
        });
        return Task.CompletedTask;
    }
}