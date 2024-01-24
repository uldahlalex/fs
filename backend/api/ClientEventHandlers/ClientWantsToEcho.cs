using api.Abstractions;
using api.Models;
using api.Models.ServerEvents;
using api.StaticHelpers.ExtensionMethods;
using Fleck;

namespace api.ClientEventHandlers;

public class ClientWantsToEchoDto :BaseDto
{
    public string message { get; set; } = null!;
}

public class ClientWantsToEcho : BaseEventHandler<ClientWantsToEchoDto>
{
    public override Task Handle(ClientWantsToEchoDto dto, IWebSocketConnection socket)
    {
        socket.SendDto(new ServerEchosClient()
        {
            message = dto.message
        });
        return Task.CompletedTask;
    }
}