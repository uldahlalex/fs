namespace core;

public class ClientSendsMessageToRoom: BaseTransferObject
{

    public string messageContent { get; set; }
    public int roomId { get; set; }
    
}


public class ServerBroadcastsMessageToClients : BaseTransferObject
{

    public Message message;
    
}

