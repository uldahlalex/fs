using Fleck;
using Serilog;

namespace core.ExtensionMethods;

public static class WebsocketExtensions
{
    private static readonly HashSet<IWebSocketConnection> AuthenticatedConnections = new();
    private static readonly Dictionary<IWebSocketConnection, List<int>> ConnectedRooms = new();

    public static void Authenticate(this IWebSocketConnection connection)
    {
        // Add the connection to the HashSet. If it's already authenticated, this will have no effect.
        AuthenticatedConnections.Add(connection);
    }

    public static void UnAuthenticate(this IWebSocketConnection connection)
    {
        // Remove the connection from the HashSet. If it's not authenticated, this will have no effect.
        AuthenticatedConnections.Remove(connection);
    }

    // You could add a method to check if a connection is authenticated.
    public static bool IsAuthenticated(this IWebSocketConnection connection)
    {
        // This will return true if the connection is authenticated, and false otherwise.
        return AuthenticatedConnections.Contains(connection);
    }

    public static void JoinRoom(this IWebSocketConnection connection, int roomId)
    {
        if (!ConnectedRooms.ContainsKey(connection))
            ConnectedRooms[connection] = new List<int>();
        Log.Information("Joining room: "+roomId+" for connection: "+connection.ConnectionInfo.Id+"");
        ConnectedRooms[connection].Add(roomId);
    }

    public static List<int> GetConnectedRooms(this IWebSocketConnection connection)
    {
        if (!ConnectedRooms.ContainsKey(connection))
            return new List<int>();

        return ConnectedRooms[connection];
    }

    public static void RemoveFromRoom(this IWebSocketConnection connection, int roomId)
    {
        if (!ConnectedRooms.ContainsKey(connection))
            return;

        ConnectedRooms[connection].Remove(roomId);
    }
}