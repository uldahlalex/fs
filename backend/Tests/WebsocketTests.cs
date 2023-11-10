using System.Net.WebSockets;
using System.Text;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace Tests;

public class Tests
{

    private string WebSocketServerUri = "ws://localhost:5000/api/room1";
    
    [Test]
    public async Task TwoClientsCanConnectAndSendMessage()
    {
        using var clientWebSocket1 = new ClientWebSocket();
        using var clientWebSocket2 = new ClientWebSocket();
        
        await clientWebSocket1.ConnectAsync(new Uri(WebSocketServerUri), CancellationToken.None);
        await clientWebSocket2.ConnectAsync(new Uri(WebSocketServerUri), CancellationToken.None);

        Assert.That(clientWebSocket1.State, Is.EqualTo(WebSocketState.Open));
        Assert.That(clientWebSocket2.State, Is.EqualTo(WebSocketState.Open));
        
        // Client 1 sends a message
        var obj = new { messageContent = "Hello from client 1" };
        var objectString =
            JsonConvert.SerializeObject(obj);
        var buffer = Encoding.UTF8.GetBytes(objectString);
        await clientWebSocket1.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        
        // Client 2 receives a message
        buffer = new byte[1024];
        var result = await clientWebSocket2.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
        /*Console.WriteLine(receivedMessage);
        var opts = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var deSerializedMessage = JsonSerializer.Deserialize<Message>(receivedMessage, opts);*/
        await clientWebSocket1.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
        await clientWebSocket2.CloseAsync(WebSocketCloseStatus.Empty, null, CancellationToken.None);
        
        receivedMessage.Should().BeEquivalentTo("\"A client has entered the room!\"");
    }
}

public class Message
{
    public int Id { get; set; }
    public string MessageContent { get; set; }
}