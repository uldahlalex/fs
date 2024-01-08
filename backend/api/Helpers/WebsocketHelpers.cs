using api.Extensions;
using api.Models;
using api.Models.Enums;
using api.State;

namespace api.Helpers;

public static class WebsocketHelpers
{
    public static void BroadcastObjectToTopicListeners<T>(T dto, TopicEnums topic) where T : BaseDto
    {
        if (WebsocketConnections.TopicSubscriptions.TryGetValue(topic, out var connections))
            foreach (var connectionId in connections)
                if (WebsocketConnections.ConnectionPool.TryGetValue(connectionId, out var socketMetadata))
                {
                    socketMetadata.Socket!.SendDto(dto);
                }
    }
}