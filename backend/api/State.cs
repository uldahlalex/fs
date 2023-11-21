using System.Collections.Concurrent;
using Fleck;

namespace api;

public class State
{
    public readonly Dictionary<Guid, IWebSocketConnection> _allSockets = new(); //todo refactor til liste sorteret efter guid?
}