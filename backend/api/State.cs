using System.Collections.Concurrent;
using Fleck;

namespace api;

public class State
{
    public readonly Dictionary<Guid, IWebSocketConnection> _allSockets = new(); //ext method refactor

    public readonly ConcurrentDictionary<int, List<Guid>> _socketsConnectedToRoom = new(
        new List<KeyValuePair<int, List<Guid>>>
        {
            new(1, new List<Guid>()),
            new(2, new List<Guid>()),
            new(3, new List<Guid>()),
        });

}