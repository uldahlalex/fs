using api.Models.ServerEvents;
using api.StaticHelpers.ExtensionMethods;
using Fleck;
using lib;

namespace api.ClientEventHandlers;

public class ClientWantsToSendBase64EncodedDataDto : BaseDto
{
    public string base64EncodedData { get; set; } = null!;
}

public class ClientWantsToSendBase64EncodedData : BaseEventHandler<ClientWantsToSendBase64EncodedDataDto>
{
    public override Task Handle(ClientWantsToSendBase64EncodedDataDto dto, IWebSocketConnection socket)
    {
        var resp = new ServerSendsBase64EncodedData()
        {
base64EncodedData = dto.base64EncodedData
        };
        socket.SendDto(resp); //todo Echo to test transmission of image
        return Task.CompletedTask;
    }
}