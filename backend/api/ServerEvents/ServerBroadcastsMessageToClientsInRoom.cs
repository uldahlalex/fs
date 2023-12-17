using api.SharedApiModels;
using Infrastructure.QueryModels;

namespace api.ServerEvents;

public class ServerBroadcastsMessageToClientsInRoom : BaseTransferObject
{
    public MessageWithSenderEmail? message { get; set; }
    public int roomId { get; set; }
}