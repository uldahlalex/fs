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
    private string WebSocketServerUri = "ws://localhost:8181/room1";

    [Test]
    public async Task TwoClientsCanConnectAndSendMessage()
    {
        var client = new ClientWebSocket();
        await client.ConnectAsync(new Uri("ws://localhost:8181"), CancellationToken.None);

        var client2 = new ClientWebSocket();
        await client2.ConnectAsync(new Uri("ws://localhost:8181"), CancellationToken.None);

        // Send a message
        var msg = new { MessageContent = "Hello, server!" };
        var msgString = JsonConvert.SerializeObject(msg);
        var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(msgString));
        await client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

        // Receive the response
        var receivedBuffer = new ArraySegment<byte>(new byte[1024]);
        var result = await client2.ReceiveAsync(receivedBuffer, CancellationToken.None);

        var actualResponse = Encoding.UTF8.GetString(receivedBuffer.Array, 0, result.Count);
        Message message = JsonSerializer.Deserialize<Message>(actualResponse,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            }) ?? throw new InvalidOperationException("Could not deserialize into a Message object");
        // Verify the server sent back the same message we sent it
        message.MessageContent.Should().BeEquivalentTo(msg.MessageContent);
    }
}

public class Message
{
    public int Id { get; set; }
    public string MessageContent { get; set; }
}