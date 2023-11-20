using Fleck;

namespace api;

public static class WebsocketExtensionMethods
{
    private static readonly Dictionary<IWebSocketConnection, bool> AuthStates = new();
    private static readonly Dictionary<IWebSocketConnection, List<int>> ConnectedToRooms = new();

    public static void SetAuthentication(this IWebSocketConnection connection, bool isAuthenticated)
    {
        AuthStates[connection] = isAuthenticated;
    }

    public static bool IsAuthenticated(this IWebSocketConnection connection)
    {
        return AuthStates.TryGetValue(connection, out var isAuthenticated) && isAuthenticated;
    }

    public static bool IsInRoom(this IWebSocketConnection connection)
    {
        throw new NotImplementedException();
    }
    
    
    public static bool AddToRoom(this IWebSocketConnection connection)
    {
        throw new NotImplementedException();
    }
}