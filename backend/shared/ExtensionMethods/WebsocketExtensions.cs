using System.Collections.Concurrent;
using core.Models;
using Fleck;

namespace core.ExtensionMethods;

public static class WebsocketExtensions
{
    //mit rationale med auth som ext method var at state var på objektet, men det er det jo ikke eftersom det er statisk
    //så jeg kan lige så godt lave det andetsteds

    //Lav en til metadata - eller eventuelt en dictionary et sted
    public static ConcurrentDictionary<Guid, WebSocketInfo> LiveConnections = new();


    public static void Authenticate(this IWebSocketConnection connection, EndUser userInfo)
    {
        // Add the connection to the HashSet. If it's already authenticated, this will have no effect.
        //_connectionBelongsToUserWithId = userId;
        //AuthenticatedConnections.Add(connection);
        LiveConnections[connection.ConnectionInfo.Id].isAuthenticated = true;
        LiveConnections[connection.ConnectionInfo.Id].userInfo = userInfo;
    }

    public static void UnAuthenticate(this IWebSocketConnection connection)
    {
        // Remove the connection from the HashSet. If it's not authenticated, this will have no effect.
        //AuthenticatedConnections.Remove(connection);
        LiveConnections[connection.ConnectionInfo.Id].isAuthenticated = false;
    }

    // You could add a method to check if a connection is authenticated.
    public static bool IsAuthenticated(this IWebSocketConnection connection)
    {
        // This will return true if the connection is authenticated, and false otherwise.
        return LiveConnections[connection.ConnectionInfo.Id].isAuthenticated;
    }

    public static void JoinRoom(this IWebSocketConnection connection, int roomId)
    {
        /* if (!ConnectedRooms.ContainsKey(connection))
             ConnectedRooms[connection] = new List<int>();
         Log.Information("Joining room: "+roomId+" for connection: "+connection.ConnectionInfo.Id+"");
         ConnectedRooms[connection].Add(roomId);*/
        LiveConnections[connection.ConnectionInfo.Id].connectedRooms.Add(roomId);
    }

    public static List<int> GetConnectedRooms(this IWebSocketConnection connection)
    {
        /* if (!ConnectedRooms.ContainsKey(connection))
             return new List<int>();

         return ConnectedRooms[connection];*/
        return LiveConnections[connection.ConnectionInfo.Id].connectedRooms.ToList();
    }

    public static void RemoveFromRoom(this IWebSocketConnection connection, int roomId)
    {
        /*if (!ConnectedRooms.ContainsKey(connection))
            return;

        ConnectedRooms[connection].Remove(roomId);*/
        LiveConnections[connection.ConnectionInfo.Id].connectedRooms.Remove(roomId);
    }

    public static bool IsInRoom(this IWebSocketConnection connection, int roomId)
    {
        /*if (!ConnectedRooms.ContainsKey(connection))
            return false;

        return ConnectedRooms[connection].Contains(roomId);*/
        return LiveConnections[connection.ConnectionInfo.Id].connectedRooms.Contains(roomId);
    }
}

public class WebSocketInfo
{
    public IWebSocketConnection? Socket { get; set; }

    public bool isAuthenticated { get; set; }

    //skal isbanned med?
    public EndUser userInfo { get; set; }
    public HashSet<int> connectedRooms { get; set; }
}