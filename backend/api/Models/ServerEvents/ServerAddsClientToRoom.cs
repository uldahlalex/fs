using api.Models;
using Infrastructure.QueryModels;

namespace api.ClientEventHandlers;

public class ServerAddsClientToRoom : BaseTransferObject
{
    public int roomId { get; set; }
    public int liveConnections { get; set; }
    public IEnumerable<MessageWithSenderEmail>? messages { get; set; }
}