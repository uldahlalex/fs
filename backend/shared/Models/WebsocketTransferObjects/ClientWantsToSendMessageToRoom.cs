using core.Attributes;

namespace core.Models.WebsocketTransferObjects;

public class ClientWantsToSendMessageToRoom : BaseTransferObject
{
    [ToxicityFilter] public string? messageContent { get; set; }

    public int roomId { get; set; }
}