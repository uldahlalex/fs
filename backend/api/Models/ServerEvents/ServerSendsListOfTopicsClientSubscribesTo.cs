using api.Models.Enums;

namespace api.Models.ServerEvents;

public class ServerSendsListOfTopicsClientSubscribesTo : BaseDto
{
    public List<TopicEnums> topics { get; set; } = new();
}