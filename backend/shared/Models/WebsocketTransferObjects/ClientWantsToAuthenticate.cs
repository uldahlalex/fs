using System.ComponentModel.DataAnnotations;

namespace core.Models.WebsocketTransferObjects;

public class ClientWantsToAuthenticate : BaseTransferObject
{
    [EmailAddress]
    public string? email { get; set; }
    [MinLength(6)]
    public string? password { get; set; }
}