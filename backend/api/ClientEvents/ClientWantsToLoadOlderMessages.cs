using api;

namespace core.Models.WebsocketTransferObjects;

public class ClientWantsToLoadOlderMessages : BaseTransferObject
{
    public int roomId { get; set; }
    public int lastMessageId { get; set; }
}