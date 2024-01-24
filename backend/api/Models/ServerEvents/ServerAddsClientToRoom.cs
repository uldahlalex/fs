using Externalities.QueryModels;

namespace api.Models.ServerEvents;

public class ServerAddsClientToRoom : BaseDto
{
    public int roomId { get; set; }
    public int liveConnections { get; set; }
    public IEnumerable<MessageWithSenderEmail>? messages { get; set; }
}