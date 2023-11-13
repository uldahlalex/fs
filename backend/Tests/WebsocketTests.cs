using core;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Websocket.Client;
using JsonSerializer = System.Text.Json.JsonSerializer;
using JsonSerializerOptions = System.Text.Json.JsonSerializerOptions;

namespace Tests;

public class Tests
{
    private const string WebSocketServerUri = "ws://localhost:8181/room1";

    [Test]
    public async Task TwoClientsCanConnectAndSendMessage()
    {
        var receivedResponses = new List<List<Message>>();
        using (var client = new WebsocketClient(new Uri(WebSocketServerUri)))
        {
            client.MessageReceived.Subscribe(msg =>
            {
                List<Message> response = JsonSerializer.Deserialize<List<Message>>(msg.Text ?? throw new InvalidOperationException("Could not deserialize into a List<Message>"),
                                             new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                         ?? throw new ArgumentException("Could not convert to MessageDto");
                receivedResponses.Add(response);
            });
            await client.Start();

            // Send a message
            var msg = new { MessageContent = "Hello, server!" };
            var msgString = JsonConvert.SerializeObject(msg);
            client.Send(msgString);
        }

        // Wait to receive the responses
        await Task.Delay(TimeSpan.FromSeconds(1));

        receivedResponses[0][0].Should().Be("PAST_MESSAGES");
        receivedResponses[1][0].messageContent.Should().Be("Hello, server!");
    }
}
