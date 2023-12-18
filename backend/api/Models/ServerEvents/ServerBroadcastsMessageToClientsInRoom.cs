using api.Models.QueryModels;

namespace api.Models.ServerEvents;

public class ServerBroadcastsMessageToClientsInRoom : BaseTransferObject
{
    public MessageWithSenderEmail? message { get; set; }
    public int roomId { get; set; }
}