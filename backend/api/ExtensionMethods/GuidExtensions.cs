using System.Collections.Concurrent;
using core.State;

namespace core.ExtensionMethods;

public static class GuidExtensions
{
    public static void SubscribeToTopic(this Guid connectionId, string topic)
    {
        if (!WebsocketConnections.TopicSubscriptions.ContainsKey(topic))
        {
            WebsocketConnections.TopicSubscriptions.TryAdd(topic, new ConcurrentBag<Guid>());
        }

        WebsocketConnections.TopicSubscriptions[topic].Add(connectionId);
    }

    public static void UnsubscribeFromTopic(this Guid connectionId, string topic)
    {
        if (WebsocketConnections.TopicSubscriptions.TryGetValue(topic, out var bag))
        {
            var newBag = new ConcurrentBag<Guid>(bag.Where(id => id != connectionId));
            WebsocketConnections.TopicSubscriptions[topic] = newBag;
        }
    }
}