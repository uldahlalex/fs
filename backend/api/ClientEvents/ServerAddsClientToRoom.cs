using api.SharedApiModels;
using Infrastructure.QueryModels;

namespace api.ClientEvents;

public class ServerAddsClientToRoom : BaseTransferObject
{
    public int roomId { get; set; }
    public int liveConnections { get; set; }
    public IEnumerable<MessageWithSenderEmail>? messages { get; set; }
}