namespace core.Models.WebsocketTransferObjects;

public class ServerAddsClientToRoom : BaseTransferObject
{

    public int roomId { get; set; }
    public int liveConnections { get; set; }
    public IEnumerable<Message>? messages { get; set; }
}

