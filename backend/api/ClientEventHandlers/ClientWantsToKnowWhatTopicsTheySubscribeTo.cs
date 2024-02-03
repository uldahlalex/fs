using api.Attributes.EventFilters;
using api.Models.ServerEvents;
using api.State;
using api.StaticHelpers.ExtensionMethods;
using Fleck;
using lib;

namespace api.ClientEventHandlers;

public class ClientWantsToKnowWhatTopicsTheySubscribeToDto : BaseDto;

[RequireAuthentication]
[RateLimit(5, 60)]
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