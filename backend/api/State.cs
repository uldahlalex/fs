using System.Collections.Concurrent;
using Fleck;

namespace api;

public class State
{ 
    public readonly ConcurrentDictionary<Guid, IWebSocketConnection> AllSockets = new(); 
}