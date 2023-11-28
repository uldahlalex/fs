namespace core.Models.WebsocketTransferObjects;

public class ServerSendsErrorMessageToClient : BaseTransferObject
{

    public string? errorMessage { get; set; }
    public string? receivedEventType { get; set; }
}

