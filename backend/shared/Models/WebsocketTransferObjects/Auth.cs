namespace core.Models.WebsocketTransferObjects;

public class ClientWantsToRegister : BaseTransferObject
{
    public string? email { get; set; }
    public string? password { get; set; }
}