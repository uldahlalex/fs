namespace api.Models.ServerEvents;

public class ServerSendsErrorMessageToClient : BaseTransferObject
{
    public string? errorMessage { get; set; }
    public string? receivedEventType { get; set; }
}