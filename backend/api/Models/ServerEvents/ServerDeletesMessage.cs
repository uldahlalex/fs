using lib;

namespace api.Models.ServerEvents;

public class ServerDeletesMessage : BaseDto
{
    public int messageId { get; set; }
    public int roomId { get; set; }
}