namespace core.Models.WebsocketTransferObjects;

public class ServerSendsOlderMessagesToClient : BaseTransferObject
{
    public IEnumerable<MessageWithSenderEmail>? messages { get; set; }
    public int roomId { get; set; }
}