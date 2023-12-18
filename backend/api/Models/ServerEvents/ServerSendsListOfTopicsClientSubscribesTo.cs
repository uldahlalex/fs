using api.Models.Enums;

namespace api.Models.ServerEvents;

public class ServerSendsListOfTopicsClientSubscribesTo : BaseTransferObject
{
    public List<TopicEnums> topics { get; set; } = new();
}