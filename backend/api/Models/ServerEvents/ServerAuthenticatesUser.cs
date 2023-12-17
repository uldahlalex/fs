using api.SharedApiModels;

namespace api.ServerEvents;

public class ServerAuthenticatesUser : BaseTransferObject
{
    public string? jwt { get; set; }
}