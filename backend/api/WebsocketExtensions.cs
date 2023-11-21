using System.Runtime.CompilerServices;
using Fleck;

namespace api;

//extensions eller subclass?

public static class WebsocketExtensions
{
    private static readonly Dictionary<IWebSocketConnection, bool> AuthenticationStatus = new();
    private static readonly Dictionary<IWebSocketConnection, List<int>> ConnectedRooms = new();

    public static void Authenticate(this IWebSocketConnection connection)
    {
        if (!AuthenticationStatus.ContainsKey(connection))
            AuthenticationStatus[connection] = false; // default to true when not present
        else
            AuthenticationStatus[connection] = true;
    }

    public static bool IsAuthenticated(this IWebSocketConnection connection)
    {
        if (!AuthenticationStatus.ContainsKey(connection))
            return false; // default to false when not present

        return AuthenticationStatus[connection];
    }

    public static void JoinRoom(this IWebSocketConnection connection, int roomId)
    {
        if (!ConnectedRooms.ContainsKey(connection))
            ConnectedRooms[connection] = new List<int>();

        ConnectedRooms[connection].Add(roomId);
    }

    public static List<int> GetConnectedRooms(this IWebSocketConnection connection)
    {
        if (!ConnectedRooms.ContainsKey(connection)) 
            return new List<int>();  // default to an empty list when not present
            
        return ConnectedRooms[connection];    
    }
    public static void RemoveFromRoom(this IWebSocketConnection connection, int roomId)
    {
        if (!ConnectedRooms.ContainsKey(connection))
            return;

        ConnectedRooms[connection].Remove(roomId);
    }
}