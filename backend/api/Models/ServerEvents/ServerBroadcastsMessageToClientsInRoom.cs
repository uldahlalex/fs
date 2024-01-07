using api.Models.QueryModels;

namespace api.Models.ServerEvents;

public class ServerBroadcastsMessageToClientsInRoom : BaseDto
{
    public MessageWithSenderEmail? message { get; set; }
    public int roomId { get; set; }
}