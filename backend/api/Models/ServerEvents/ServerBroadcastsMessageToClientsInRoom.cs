using Externalities.QueryModels;
using lib;

namespace api.Models.ServerEvents;

public class ServerBroadcastsMessageToClientsInRoom : BaseDto
{
    public MessageWithSenderEmail? message { get; set; }
    public int roomId { get; set; }
}