using api.Extensions;
using api.Models;
using api.Models.Enums;
using api.State;

namespace api.Helpers;

public static class WebsocketHelpers
{
    public static void BroadcastObjectToTopicListeners(BaseDto dto, TopicEnums topic)
    {
        if (WebsocketConnections.TopicSubscriptions.TryGetValue(topic, out var connections))
            foreach (var connectionId in connections)
                if (WebsocketConnections.ConnectionPool.TryGetValue(connectionId, out var socketMetadata))
                {
                    Console.WriteLine(socketMetadata.Socket!.ConnectionInfo.Id);
                    socketMetadata.Socket!.SendDto(dto);
                }
    }
}