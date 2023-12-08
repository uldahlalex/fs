namespace core.Models.WebsocketTransferObjects;

public class ServerBroadcastsMessageToClientsInRoom : BaseTransferObject
{
    public MessageWithSenderEmail? message { get; set; }
    public int roomId { get; set; }
}