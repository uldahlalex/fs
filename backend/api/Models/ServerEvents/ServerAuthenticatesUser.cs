using lib;

namespace api.Models.ServerEvents;

public class ServerAuthenticatesUser : BaseDto
{
    public string? jwt { get; set; }
}