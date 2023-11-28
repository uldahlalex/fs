namespace core.Models.WebsocketTransferObjects;

public class ServerSendsOlderMessagesToClient : BaseTransferObject
{
    public IEnumerable<Message>? messages { get; set; }
    public int roomId { get; set; }
}