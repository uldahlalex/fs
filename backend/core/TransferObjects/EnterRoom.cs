namespace core;

public class ClientWantsToEnterRoom : BaseTransferObject
{

    public int roomId { get; set; }
}

public class ServerLetsClientEnterRoom : BaseTransferObject
{

    public int roomId { get; set; }
    public IEnumerable<Message> recentMessages { get; set; }
}