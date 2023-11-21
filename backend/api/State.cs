using System.Collections.Concurrent;
using Fleck;

namespace api;

public class State
{
    //Concurrent dictionary for super fast lookup times
    public readonly ConcurrentDictionary<Guid, IWebSocketConnection> AllSockets = new(); 
}