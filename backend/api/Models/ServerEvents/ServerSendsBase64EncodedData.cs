using lib;

namespace api.Models.ServerEvents;

public class ServerSendsBase64EncodedData : BaseDto
{
    public string base64EncodedData { get; set; } = null!;
}