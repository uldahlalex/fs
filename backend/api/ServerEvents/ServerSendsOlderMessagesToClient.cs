using api.SharedApiModels;
using Infrastructure.QueryModels;

namespace api.ServerEvents;

public class ServerSendsOlderMessagesToClient : BaseTransferObject
{
    public IEnumerable<MessageWithSenderEmail>? messages { get; set; }
    public int roomId { get; set; }
}