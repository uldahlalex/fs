namespace core.Models.WebsocketTransferObjects;

public class ServerBroadcastsMessageToClientsInRoom : BaseTransferObject
{
    public Message? message { get; set; }
    public int roomId { get; set; }
}