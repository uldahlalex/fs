namespace core.Models.WebsocketTransferObjects;

public class ServerAddsClientToRoom : BaseTransferObject
{

    public int roomId { get; set; }
    public IEnumerable<Message>? recentMessages { get; set; }
}

