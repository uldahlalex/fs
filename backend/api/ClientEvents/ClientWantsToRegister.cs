using System.ComponentModel.DataAnnotations;
using api;

namespace core.Models.WebsocketTransferObjects;

public class ClientWantsToRegister : BaseTransferObject
{
    [EmailAddress] public string? email { get; set; }

    [MinLength(6)] public string? password { get; set; }
}