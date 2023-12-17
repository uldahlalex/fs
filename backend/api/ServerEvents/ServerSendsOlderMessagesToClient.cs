using api.SharedApiModels;
using core.Models.QueryModels;

namespace api.ServerEvents;

public class ServerSendsOlderMessagesToClient : BaseTransferObject
{
    public IEnumerable<MessageWithSenderEmail>? messages { get; set; }
    public int roomId { get; set; }
}