using Fleck;

namespace api;

public static class WebsocketServer
{
    public static void start()
    {
        var server = new WebSocketServer("ws://127.0.0.1:8181");
        server.Start(socket =>
        {
            socket.OnOpen = () => Console.WriteLine("Open!");
            socket.OnClose = () => Console.WriteLine("Close!");
            socket.OnMessage = message => socket.Send(message);
        });

    }
}