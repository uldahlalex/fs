namespace core;

public class ServerSendsErrorMessageToClient : BaseTransferObject
{

    public string errorMessage { get; set; }
    public string receivedEventType { get; set; }
}

