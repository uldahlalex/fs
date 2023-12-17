using api.SharedApiModels;

namespace api.ServerEvents;

public class ServerSendsErrorMessageToClient : BaseTransferObject
{
    public string? errorMessage { get; set; }
    public string? receivedEventType { get; set; }
}