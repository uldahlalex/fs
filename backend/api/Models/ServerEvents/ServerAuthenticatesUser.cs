namespace api.Models.ServerEvents;

public class ServerAuthenticatesUser : BaseTransferObject
{
    public string? jwt { get; set; }
}