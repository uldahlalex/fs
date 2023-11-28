namespace core.Models.WebsocketTransferObjects;

public class ServerAuthenticatesUser : BaseTransferObject
{
    public string? jwt { get; set; }
}