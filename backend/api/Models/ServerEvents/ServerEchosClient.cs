namespace api.Models.ServerEvents;

public class ServerEchosClient : BaseDto
{
    public string message { get; set; } = null!;
}