using api.Models.Enums;
using api.State;
using api.StaticHelpers.ExtensionMethods;
using lib;

namespace api.StaticHelpers;

public static class StaticWebSocketHelpers
{
    public static void BroadcastObjectToTopicListeners<T>(T dto, TopicEnums topic) where T : BaseDto
    {
        if (WebsocketConnections.TopicSubscriptions.TryGetValue(topic, out var connections))
            foreach (var connectionId in connections)
                if (WebsocketConnections.ConnectionPool.TryGetValue(connectionId, out var socketMetadata))
                    socketMetadata.Socket!.SendDto(dto);
    }
}