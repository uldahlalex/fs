namespace api.Models.ServerEvents;

public class ServerSendsErrorMessageToClient : BaseDto
{
    public string? errorMessage { get; set; }
    public string? receivedEventType { get; set; }
}