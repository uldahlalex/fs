using api.Models.Enums;

namespace api.StaticHelpers.ExtensionMethods;

public static class TopicExtensions
{
    public static TopicEnums ParseTopicFromRoomId(this int roomId)
    {
        var isValidTopic = Enum.TryParse("ChatRoom" + roomId, out TopicEnums topic);
        if (!isValidTopic)
            throw new ArgumentException("Invalid topic");
        return topic;
    }
}