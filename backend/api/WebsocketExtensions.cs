using System.Runtime.CompilerServices;
using Fleck;

namespace api;

//extensions eller subclass?

public static class WebsocketExtensions
{
    public static Dictionary<IWebSocketConnection, bool> authenticationStatus = new();
    public static Dictionary<IWebSocketConnection, List<int>> connectedRooms = new();

    public static void Authenticate(this IWebSocketConnection connection)
    {
        if (!authenticationStatus.ContainsKey(connection))
            authenticationStatus[connection] = true; // default to true when not present
        else
            authenticationStatus[connection] = true;
    }

    public static bool IsAuthenticated(this IWebSocketConnection connection)
    {
        if (!authenticationStatus.ContainsKey(connection))
            return false; // default to false when not present

        return authenticationStatus[connection];
    }

    public static void JoinRoom(this IWebSocketConnection connection, int roomId)
    {
        if (!connectedRooms.ContainsKey(connection))
            connectedRooms[connection] = new List<int>();

        connectedRooms[connection].Add(roomId);
    }

    public static List<int> ConnectedRooms(this IWebSocketConnection connection)
    {
        if (!connectedRooms.ContainsKey(connection)) 
            return new List<int>();  // default to an empty list when not present
            
        return connectedRooms[connection];    
    }
    public static void RemoveFromRoom(this IWebSocketConnection connection, int roomId)
    {
        if (!connectedRooms.ContainsKey(connection))
            return;

        connectedRooms[connection].Remove(roomId);
    }
}