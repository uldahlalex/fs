using api.SharedApiModels;
using core.Models.QueryModels;

namespace api.ServerEvents;

public class ServerBroadcastsMessageToClientsInRoom : BaseTransferObject
{
    public MessageWithSenderEmail? message { get; set; }
    public int roomId { get; set; }
}