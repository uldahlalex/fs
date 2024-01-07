using api.Models.QueryModels;

namespace api.Models.ServerEvents;

public class ServerSendsOlderMessagesToClient : BaseDto
{
    public IEnumerable<MessageWithSenderEmail>? messages { get; set; }
    public int roomId { get; set; }
}